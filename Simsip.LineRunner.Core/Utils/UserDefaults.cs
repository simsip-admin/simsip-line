using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.IO;
using Simsip.LineRunner.GameFramework;


namespace Simsip.LineRunner.Utils
{
    /// <summary>
    /// IMPORTANT: All new enum types have to be added to bottom so that 
    /// upgrades will still read in the correct enum type. See LoadSettings()
    /// where we cast an int to this enum - hence we need to keep order of these
    /// enums locked.
    /// </summary>
    public enum UserDefaultType
    {
        BoolType,
        DateType,
        DoubleType,
        FloatType,
        IntegerType,
        StringType
    }

    public class UserDefaultEntry
    {
        public string Key;
        public UserDefaultType TheType;
        public object Value;
    }

    public class UserDefaults
    {
        private const string SettingsFilename = "Settings";

        private Dictionary<string, UserDefaultEntry> _userDefaultEntries;

        /// <summary>
        /// A singleton implementation of our user defaults.
        /// 
        ///  IMPORTANT: We only write out settings if they deviate from a default for each setting. 
        ///  The GetXXX api signattures all require a default to be passed if we do not find an entry recorded.
        ///  
        /// We also use the async Factory Pattern as described here:
        /// http://blog.stephencleary.com/2013/01/async-oop-2-constructors.html
        /// </summary>
        public static UserDefaults SharedUserDefault;
        
        // Singleton
        private UserDefaults()
        {
            // Initialize state
            this._userDefaultEntries = new Dictionary<string, UserDefaultEntry>();
        }

        #region Api

#if ANDROID || IOS || DESKTOP
        public static void Initialize()
#elif WINDOWS_PHONE || NETFX_CORE
        public static async Task Initialize()
#endif
        {
            // Singleton
            UserDefaults.SharedUserDefault = new UserDefaults();

            // Load current settings, if any, into _userDefaultEntries
#if ANDROID || IOS || DESKTOP
            UserDefaults.SharedUserDefault.LoadSettings();
#elif WINDOWS_PHONE || NETFX_CORE
            await UserDefaults.SharedUserDefault.LoadSettings();
#endif

        }

        public bool ContainsKey(string key)
        {
            return this._userDefaultEntries.ContainsKey(key);
        }

        public bool GetBoolForKey(string key, bool defaultBool)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (bool)this._userDefaultEntries[key].Value;
            }

            return defaultBool;
        }

        public DateTime GetDateForKey(string key, DateTime defaultDate)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (DateTime)this._userDefaultEntries[key].Value;
            }

            return defaultDate;
        }

        public double GetDoubleForKey(string key, double defaultDouble)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (double)this._userDefaultEntries[key].Value;
            }

            return defaultDouble;
        }

        public float GetFloatForKey(string key, float defaultFloat)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (float)this._userDefaultEntries[key].Value;
            }

            return defaultFloat;
        }

        public int GetIntegerForKey(string key, int defaultInteger)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (int)this._userDefaultEntries[key].Value;
            }

            return defaultInteger;
        }

        public string GetStringForKey(string key, string defaultString)
        {
            if (this._userDefaultEntries.ContainsKey(key))
            {
                return (string)this._userDefaultEntries[key].Value;
            }

            return defaultString;
        }

        public void SetBoolForKey(string key, bool value)
        {
            SetValueForKey(key, value, UserDefaultType.BoolType);
        }

        public void SetDateForKey(string key, DateTime value)
        {
            SetValueForKey(key, value, UserDefaultType.DateType);
        }

        public void SetDoubleForKey(string key, double value)
        {
            SetValueForKey(key, value, UserDefaultType.DoubleType);
        }

        public void SetFloatForKey(string key, float value)
        {
            SetValueForKey(key, value, UserDefaultType.FloatType);
        }

        public void SetIntegerForKey(string key, int value)
        {
            SetValueForKey(key, value, UserDefaultType.IntegerType);
        }

        public void SetStringForKey(string key, string value)
        {
            SetValueForKey(key, value, UserDefaultType.StringType);
        }


        #endregion

        #region Helper methods

#if ANDROID || IOS || DESKTOP
        private void LoadSettings()
#elif WINDOWS_PHONE || NETFX_CORE
        private async Task LoadSettings()
#endif
        {
            // Short-circuit if we have not recorded any user defaults yet. (We only record if they
            // deviate from defined defaults).
            var settingsPath = Path.Combine(GameConstants.FOLDER_CONFIG, UserDefaults.SettingsFilename);
#if ANDROID || IOS || DESKTOP
            if (!FileUtils.FileExists(settingsPath))
#elif WINDOWS_PHONE || NETFX_CORE
            if (! await FileUtils.FileExistsAsync(settingsPath))
#endif
            {
                return;
            }

#if ANDROID || IOS || DESKTOP
            var stringSettings = FileUtils.LoadText(settingsPath);
#elif WINDOWS_PHONE || NETFX_CORE
            var stringSettings = await FileUtils.LoadTextAsync(settingsPath);
#endif
            
            var jsonSettings = JObject.Parse(stringSettings);
            int settingsCount = jsonSettings["settings"].Count();
            for (int i = 0; i < settingsCount; i++)
            {
                // Make sure one bad chunk doesn't spoil it for everyone else
                try
                {
                    // Extract key and type
                    var jsonSetting = (JObject)jsonSettings["settings"][i];
                    var key = (string)jsonSetting["key"];
                    var typeInt = (int)jsonSetting["type"];
                    var type = (UserDefaultType)typeInt;

                    // Load value based on type
                    var userDefaultEntry = new UserDefaultEntry()
                        {
                            Key = key,
                            TheType = type
                        };
                    switch (type)
                    {
                        case UserDefaultType.BoolType:
                            {
                                userDefaultEntry.Value = (bool)jsonSetting["value"];
                                break;
                            }
                        case UserDefaultType.DateType:
                            {
                                userDefaultEntry.Value = (DateTime)jsonSetting["value"];
                                break;
                            }
                        case UserDefaultType.DoubleType:
                            {
                                userDefaultEntry.Value = (double)jsonSetting["value"];
                                break;
                            }
                        case UserDefaultType.FloatType:
                            {
                                userDefaultEntry.Value = (float)jsonSetting["value"];
                                break;
                            }
                        case UserDefaultType.IntegerType:
                            {
                                userDefaultEntry.Value = (int)jsonSetting["value"];
                                break;
                            }
                        case UserDefaultType.StringType:
                            {
                                userDefaultEntry.Value = (string)jsonSetting["value"];
                                break;
                            }
                    }

                    // Did we parse successfully?
                    if (userDefaultEntry.Value != null)
                    {
                        // Ok to add in user default
                        this._userDefaultEntries.Add(userDefaultEntry.Key, userDefaultEntry);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception loading setting: " + ex);
                }
            }
        }

#if ANDROID || IOS || DESKTOP
        private void SaveSettings()
#elif WINDOWS_PHONE || NETFX_CORE
        private async void SaveSettings()
#endif
        {
            var jsonRoot = new JObject();
            
            jsonRoot.Add("version", 1);

            var jsonSettings = new JArray();
            foreach (var setting in this._userDefaultEntries.Values)
            {
                // Write out key/type
                var jsonSetting = new JObject();
                jsonSetting.Add("key", setting.Key);
                jsonSetting.Add("type", (int)setting.TheType);
                
                // Write out value based on type
                switch (setting.TheType)
                {
                    case UserDefaultType.BoolType:
                        {
                            jsonSetting.Add("value", (bool)setting.Value);
                            break;
                        }
                    case UserDefaultType.DateType:
                        {
                            jsonSetting.Add("value", (DateTime)setting.Value);
                            break;
                        }
                    case UserDefaultType.DoubleType:
                        {
                            jsonSetting.Add("value", (double)setting.Value);
                            break;
                        }
                    case UserDefaultType.FloatType:
                        {
                            jsonSetting.Add("value", (float)setting.Value);
                            break;
                        }
                    case UserDefaultType.IntegerType:
                        {
                            jsonSetting.Add("value", (int)setting.Value);
                            break;
                        }
                    case UserDefaultType.StringType:
                        {
                            jsonSetting.Add("value", (string)setting.Value);
                            break;
                        }
                }

                // Write out json reprsentation of setting to our json array of settings
                jsonSettings.Add(jsonSetting);
            }

            // Write out json array of settings
            jsonRoot.Add("settings", jsonSettings);

            // Write out string representation of json to file, replacing the file if already there.
            var stringRoot = jsonRoot.ToString();
            var settingsPath = Path.Combine(GameConstants.FOLDER_CONFIG, UserDefaults.SettingsFilename);
#if ANDROID || IOS || DESKTOP
            if (FileUtils.FileExists(settingsPath))
            {
                FileUtils.DeleteFile(settingsPath);
            }
            FileUtils.SaveText(settingsPath, stringRoot);
#elif WINDOWS_PHONE || NETFX_CORE
            // IMPORTANT: Even though the saving is done async, the actually
            // values accessed are maintained in-memory so no need for race condition worries
            if (await FileUtils.FileExistsAsync(settingsPath))
            {
                await FileUtils.DeleteFileAsync(settingsPath);
            }
            await FileUtils.SaveTextAsync(settingsPath, stringRoot);
#endif
        }

        private void SetValueForKey(string key, object value, UserDefaultType type)
        {
            // Construct and add/update the setting
            var userDefaultEntry = new UserDefaultEntry()
            {
                Key = key,
                TheType = type,
                Value = value
            };
            this._userDefaultEntries[key] = userDefaultEntry;

            // Write out a clean set of settings to file
            this.SaveSettings();
        }

        #endregion
    }
}