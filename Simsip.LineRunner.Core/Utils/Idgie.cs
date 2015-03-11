using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simsip.LineRunner.Data.OAuth;
using Simsip.LineRunner.Entities.OAuth;
using Simsip.LineRunner.Services.OAuth;


namespace Simsip.LineRunner.Utils
{
    public static class Idgie
    {
        public static string June(int type)
        {
            var service = new OAuthAccessRepository();
            var enumType = (OAuthType) type;
            var record = service.Read(enumType);
            var offset = type % 4;

            var stringBuffer = new StringBuilder();
            switch (offset)
            {
                case 0:
                {
                    stringBuffer.Append(record.AppSecretPart1);
                    stringBuffer.Append(record.AppSecretPart3);
                    stringBuffer.Append(record.AppSecretPart5);
                    stringBuffer.Append(record.AppSecretPart7);
                    break;
                }
                case 1:
                {
                    stringBuffer.Append(record.AppSecretPart2);
                    stringBuffer.Append(record.AppSecretPart4);
                    stringBuffer.Append(record.AppSecretPart6);
                    stringBuffer.Append(record.AppSecretPart8);
                    break;
                }
                case 2:
                {
                    stringBuffer.Append(record.AppSecretPart3);
                    stringBuffer.Append(record.AppSecretPart5);
                    stringBuffer.Append(record.AppSecretPart7);
                    stringBuffer.Append(record.AppSecretPart1);
                    break;
                }
                case 3:
                {
                    stringBuffer.Append(record.AppSecretPart4);
                    stringBuffer.Append(record.AppSecretPart6);
                    stringBuffer.Append(record.AppSecretPart8);
                    stringBuffer.Append(record.AppSecretPart2);
                    break;
                }
            }

            return stringBuffer.ToString();
        }

        public static string Putnam(int type)
        {
            var service = new OAuthAccessRepository();
            var enumType = (OAuthType)type;
            var record = service.Read(enumType);
            var offset = type % 4;

            var stringBuffer = new StringBuilder();
            switch (offset)
            {
                case 0:
                    {
                        stringBuffer.Append(record.AppSecretPart2);
                        stringBuffer.Append(record.AppSecretPart4);
                        stringBuffer.Append(record.AppSecretPart6);
                        stringBuffer.Append(record.AppSecretPart8);
                        break;
                    }
                case 1:
                    {
                        stringBuffer.Append(record.AppSecretPart3);
                        stringBuffer.Append(record.AppSecretPart5);
                        stringBuffer.Append(record.AppSecretPart7);
                        stringBuffer.Append(record.AppSecretPart1);
                        break;
                    }
                case 2:
                    {
                        stringBuffer.Append(record.AppSecretPart4);
                        stringBuffer.Append(record.AppSecretPart6);
                        stringBuffer.Append(record.AppSecretPart8);
                        stringBuffer.Append(record.AppSecretPart2);
                        break;
                    }
                case 3:
                    {
                        stringBuffer.Append(record.AppSecretPart5);
                        stringBuffer.Append(record.AppSecretPart7);
                        stringBuffer.Append(record.AppSecretPart1);
                        stringBuffer.Append(record.AppSecretPart3);
                        break;
                    }
            }

            return stringBuffer.ToString();
        }
    }
}