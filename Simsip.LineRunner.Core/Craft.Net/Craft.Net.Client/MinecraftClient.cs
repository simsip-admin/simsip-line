using System;
using Craft.Net.Networking;
using Craft.Net.Physics;
using Craft.Net.Logic;
#if WINDOWS_PHONE
using Simsip.LineRunner.Concurrent;
#else
using System.Collections.Concurrent;
#endif
using System.Net;
#if !NETFX_CORE
using System.Net.Sockets;
#endif
using Craft.Net.Common;
using System.Threading;
using Craft.Net.Client.Events;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if WINDOWS_PHONE || NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.Foundation;
#endif



namespace Craft.Net.Client
{
    public partial class MinecraftClient
    {
        public delegate void PacketHandler(MinecraftClient client, IPacket packet);

        public MinecraftClient(Session session)
        {
            Session = session;
            PacketQueue = new ConcurrentQueue<IPacket>();
            PacketHandlers = new Dictionary<Type, PacketHandler>();
            Handlers.PacketHandlers.Register(this);
        }

        public Session Session { get; set; }

#if WINDOWS_PHONE || NETFX_CORE
        public StreamSocket Client { get; set; }
        protected internal DualStream NetworkStream { get; set; }
#else
        public TcpClient Client { get; set; }
        protected internal NetworkStream NetworkStream { get; set; }
#endif

        public ConcurrentQueue<IPacket> PacketQueue { get; set; }
#if NETFX_CORE
        public EndpointPair EndPoint { get; set; }
#else
        public IPEndPoint EndPoint { get; set; }
#endif

        public ReadOnlyWorld World { get; protected internal set; }
        public int EntityId { get; protected internal set; }

        protected internal NetworkManager NetworkManager { get; set; }

        internal byte[] SharedSecret { get; set; }
        internal bool IsLoggedIn { get; set; }
        internal bool IsSpawned { get; set; }

#if NETFX_CORE
        private IAsyncAction NetworkWorkerThread { get; set; }
#else
        private Thread NetworkWorkerThread { get; set; }
#endif
        private Dictionary<Type, PacketHandler> PacketHandlers { get; set; }
        private ManualResetEvent NetworkingReset { get; set; }

        public void RegisterPacketHandler(Type packetType, PacketHandler handler)
        {
#if !NETFX_CORE
            if (!packetType.GetInterfaces().Any(p => p == typeof(IPacket)))
                throw new InvalidOperationException("Packet type must implement Craft.Net.Networking.IPacket");
#endif
            PacketHandlers[packetType] = handler;
        }

#if WINDOWS_PHONE
        public async void Connect(IPEndPoint endPoint)
#elif NETFX_CORE
        public async void Connect(EndpointPair endpointPair)
#else
        public async void Connect(IPEndPoint endPoint)
#endif
        {
#if WINDOWS_PHONE || NETFX_CORE
            if (Client != null)
#else
            if (Client != null && Client.Connected)
#endif
            {
                throw new InvalidOperationException("Already connected to a server!");
            }

#if NETFX_CORE
            EndPoint = endpointPair;
#else
            EndPoint = endPoint;
#endif

#if WINDOWS_PHONE
            Client = new StreamSocket();
            var hostName = new HostName(endPoint.Address.ToString());
            var endpointPair = new EndpointPair(
                null,
                string.Empty,
                hostName, 
                endPoint.Port.ToString());
            await Client.ConnectAsync(endpointPair);
#elif NETFX_CORE
            Client = new StreamSocket();
            await Client.ConnectAsync(endpointPair);
#else
            Client = new TcpClient();
            Client.Connect(EndPoint);
            NetworkStream = Client.GetStream();
#endif
            NetworkManager = new NetworkManager(NetworkStream);
            NetworkingReset = new ManualResetEvent(true);
#if NETFX_CORE
            NetworkWorkerThread = 
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        NetworkWorker();
                    });
            PhysicsWorkerThread =
                Windows.System.Threading.ThreadPool.RunAsync(
                    (workItem) =>
                    {
                        PhysicsWorker();
                    });
#else
            NetworkWorkerThread = new Thread(NetworkWorker);
            PhysicsWorkerThread = new Thread(PhysicsWorker);
            NetworkWorkerThread.Start();
#endif


#if NETFX_CORE
             var handshake = new HandshakePacket(NetworkManager.ProtocolVersion, 
                EndPoint.RemoteHostName.ToString(), (ushort)Int32.Parse(EndPoint.RemoteServiceName), NetworkMode.Login);
#else
            var handshake = new HandshakePacket(NetworkManager.ProtocolVersion, 
                EndPoint.Address.ToString(), (ushort)EndPoint.Port, NetworkMode.Login);
#endif
            SendPacket(handshake);
            var login = new LoginStartPacket(Session.SelectedProfile.Name);
            SendPacket(login);
#if !NETFX_CORE
            PhysicsWorkerThread.Start();
#endif
        }

        public void Disconnect(string reason)
        {
#if NETFX_CORE
            NetworkWorkerThread.Cancel();
#else
            NetworkWorkerThread.Abort();
#endif
#if WINDOWS_PHONE || NETFX_CORE
            if (Client != null)
#else
            if (Client.Connected)
#endif
            {
                try
                {
                    NetworkManager.WritePacket(new DisconnectPacket(reason), PacketDirection.Serverbound);
#if WINDOWS_PHONE || NETFX_CORE
                    Client.Dispose();
#else
                    Client.Close();
#endif
                }
                catch { }
            }
        }

        public void Respawn()
        {
            if (Health > 0)
                throw new InvalidOperationException("Player is not dead!");
            SendPacket(new ClientStatusPacket(ClientStatusPacket.StatusChange.Respawn));
        }

        public void SendPacket(IPacket packet)
        {
            PacketQueue.Enqueue(packet);
        }

        public void SendChat(string message)
        {
            SendPacket(new ChatMessagePacket(message));
        }

        private DateTime nextPhysicsUpdate = DateTime.MinValue;
#if NETFX_CORE
        private IAsyncAction PhysicsWorkerThread;
#else
        private Thread PhysicsWorkerThread;
#endif
        private PhysicsEngine engine;
        private void PhysicsWorker()
        {
#if NETFX_CORE
            while (NetworkWorkerThread.Status == AsyncStatus.Started)
#else
            while (NetworkWorkerThread.IsAlive)
#endif
            {
                if (nextPhysicsUpdate < DateTime.Now)
                {
                    //We need to wait for a login packet to initialize the physics subsystem
                    if (World != null && engine == null)
                    {
                        // 50 ms / update for 20 ticks per second
                        engine = new PhysicsEngine(World.World, Block.PhysicsProvider, 50);
                        engine.AddEntity(this);
                    }
                    nextPhysicsUpdate = DateTime.Now.AddMilliseconds(50);
                    try
                    {
                        engine.Update();
                    }
                    catch (Exception)
                    {
                        // Sometimes the world hasn't loaded yet, so the Phyics update can't properly read blocks and
                        // throws an exception.
                    }
                }
                else
                {
                    var sleepTime = (nextPhysicsUpdate - DateTime.Now).Milliseconds;
                    if (sleepTime > 0)
                    {
#if !NETFX_CORE
                        Thread.Sleep(sleepTime);
#endif
                    }
                }
            }
        }

        private DateTime nextPlayerUpdate = DateTime.MinValue;
        private void NetworkWorker()
        {
            while (true)
            {
                if (IsSpawned && nextPlayerUpdate < DateTime.Now)
                {
                    nextPlayerUpdate = DateTime.Now.AddMilliseconds(100);
                    lock (_positionLock)
                    {
                        SendPacket(new PlayerPacket(OnGround));

                        if (_positionChanged)
                        {
                            SendPacket(new PlayerPositionPacket(
                                Position.X,
                                Position.Y,
                                Position.Z,
                                Position.Y - 1.62,
                                OnGround
                            ));
                            _positionChanged = false;
                        }
                    }
                }
                // Send queued packets
                while (PacketQueue.Count != 0)
                {
                    IPacket packet;
                    if (PacketQueue.TryDequeue(out packet))
                    {
                        try
                        {
                            // Write packet
                            NetworkManager.WritePacket(packet, PacketDirection.Serverbound);
                            if (packet is DisconnectPacket)
                                return;
                        }
                        catch { /* TODO */ }
                    }
                }
                // Read incoming packets
                var readTimeout = DateTime.Now.AddMilliseconds(20); // Maximum read time given to server per iteration
#if WINDOWS_PHONE || NETFX_CORE
                while (DateTime.Now < readTimeout)
#else
                while (NetworkStream.DataAvailable && DateTime.Now < readTimeout)
#endif
                {
                    try
                    {
                        var packet = NetworkManager.ReadPacket(PacketDirection.Clientbound);
                        HandlePacket(packet);
                        if (packet is DisconnectPacket)
                        {
#if !NETFX_CORE
                            Console.WriteLine(((DisconnectPacket)packet).Reason);
#endif
                            return;
                        }
                    }
                    catch (Exception e) 
                    {
#if !NETFX_CORE
                         // TODO: OnNetworkException or something
                        Console.WriteLine(e);
#endif
                    }
                }
                NetworkingReset.Set();
                NetworkingReset.Reset();
#if !NETFX_CORE
                Thread.Sleep(1);
#endif
            }
        }

        protected internal void FlushPackets()
        {
            // Writes all pending packets to the underlying network stream
            NetworkingReset.WaitOne();
        }

        private void HandlePacket(IPacket packet)
        {
            var type = packet.GetType();
            if (PacketHandlers.ContainsKey(type))
                PacketHandlers[type](this, packet);
            //throw new InvalidOperationException("Recieved a packet we can't handle: " + packet.GetType().Name);
        }

        public event EventHandler<ChatMessageEventArgs> ChatMessage;
        protected internal virtual void OnChatMessage(ChatMessageEventArgs e)
        {
            if (ChatMessage != null) ChatMessage(this, e);
        }

        public event EventHandler LoggedIn;
        protected internal virtual void OnLoggedIn()
        {
            if (LoggedIn != null) LoggedIn(this, null);
        }

        public event EventHandler<DisconnectEventArgs> Disconnected;
        protected internal virtual void OnDisconnected(DisconnectEventArgs e)
        {
            if (Disconnected != null) Disconnected(this, e);
        }

        public event EventHandler<EntitySpawnEventArgs> InitialSpawn;
        protected internal virtual void OnInitialSpawn(EntitySpawnEventArgs e)
        {
            if (InitialSpawn != null) InitialSpawn(this, e);
        }

        public event EventHandler PlayerDied;
        protected internal virtual void OnPlayerDied()
        {
            if (PlayerDied != null) PlayerDied(this, null);
        }

        public event EventHandler<HealthAndFoodEventArgs> HealthOrFoodChanged;
        protected internal virtual void OnHealthOrFoodChanged(HealthAndFoodEventArgs e)
        {
            if (HealthOrFoodChanged != null) HealthOrFoodChanged(this, e);
        }
    }
}