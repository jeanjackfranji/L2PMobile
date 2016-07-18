using System;
using System.Linq;
using L2PAPIClient;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Web;

namespace MobileL2P.Services
{
    public static class Tools
    {

        public static string pWd = "Integr$atorPa%ss123!@#$5";
        public static bool hasCookieToken = false;

        public static void getAndSetUserToken(HttpCookieCollection cookies, HttpContextBase context)
        {
            hasCookieToken = false;
            UserConfig uConfig = new UserConfig();
            HttpCookie accessToken  =  cookies.Get("CRTID");
            HttpCookie refreshToken =  cookies.Get("CRAID");
            if (accessToken != null && refreshToken != null)
            {
                Config.setAccessToken(Encryptor.Decrypt(accessToken.Value));
                Config.setRefreshToken(Encryptor.Decrypt(refreshToken.Value));
                uConfig.setAccessToken(accessToken.Value);
                uConfig.setRefreshToken(refreshToken.Value);
                hasCookieToken = true;
                context.Session.Add("LoggedIn", LoginStatus.LoggedIn);
            }
            else
            {
                context.Session.Add("LoggedIn", LoginStatus.Waiting);
            }
        }

        public static bool isUserLoggedInAndAPIActive(HttpContextBase context)
        {
            LoginStatus login_status = LoginStatus.Waiting;
            if(context.Session["LoggedIn"] != null)
            {
                login_status = (LoginStatus)context.Session["LoggedIn"];
            }
            bool l2pStatus = AuthenticationManager.getState() == AuthenticationManager.AuthenticationState.ACTIVE;
            return  (login_status.Equals(LoginStatus.LoggedIn) && l2pStatus);
        }

        public static String formatSemesterCode(String code)
        {
            return code.Replace("ss", "Summer Semester ").Replace("ws", "Winter Semester ");
        }

        public static String truncateString(String str, int truncateLength)
        {
            if(str.Length >= truncateLength)
            {
                return str.Substring(0, truncateLength);
            }
            return str;
        }

        public static string toDateString(double dateNb)
        {
            if (dateNb <= 0)
                return "";

            DateTime date = new DateTime(1970, 1, 1).AddSeconds(dateNb);
            TimeZoneInfo nInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            date = TimeZoneInfo.ConvertTimeFromUtc(date, nInfo);
            return date.ToString("dd/MM/yyyy");
        }

        public static string toDateTimeString(double dateNb)
        {
            if (dateNb <= 0)
                return "";

            DateTime date = new DateTime(1970, 1, 1).AddSeconds(dateNb);
            TimeZoneInfo nInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            date = TimeZoneInfo.ConvertTimeFromUtc(date, nInfo);
            return date.ToString("dd/MM/yyyy HH:mm");
        }

        public static string toHoursString(double dateNb)
        {
            if (dateNb <= 0)
                return "";

            DateTime date = new DateTime(1970, 1, 1).AddSeconds(dateNb);
            TimeZoneInfo nInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            date = TimeZoneInfo.ConvertTimeFromUtc(date, nInfo);
            return date.ToString("HH:mm");
        }

        public static DateTime toDateTime(double dateNb)
        {
            DateTime date = new DateTime(1970, 1, 1).AddSeconds(dateNb);
            TimeZoneInfo nInfo = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            date = TimeZoneInfo.ConvertTimeFromUtc(date, nInfo);
            return date;
        }

        public static bool checkURLValidity(string url)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        public static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();

            if (arrBytes == null)
            {
                return null;
            }

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            object obj = (object)binForm.Deserialize(memStream);
            return obj;
        }

        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T DeserializeObject<T>(Object obj)
        {
            if(obj != null)
            {
                return JsonConvert.DeserializeObject<T>(obj.ToString());
            }
            return default(T);
        }

        public static string ToFileSize(long size)
        {
            return String.Format(new FileSizeFormatProvider(), "{0:fs}", size);
        }

        public static string getImagePathByFileName(string fileName)
        {
            try
            {
                if (fileName != null)
                {
                    Match match = Regex.Match(fileName, @"$(?<=\.(pdf))", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        return "../wwwroot/images/learning_material/PDF.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(png|jpg|jpeg|gif|bmp))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/Full_Image.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(3g2|3gp|asf|asx|avi|flv|mov|mp4|mpg|rm|swf|vob|wmv))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/Video_Message.png";
                    } 
                    else if (Regex.Match(fileName, @"$(?<=\.(doc|docx|rtf))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/MS_Word.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(log|txt|wpd|wps))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/Text_Document.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(csv|xls|xlsx))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/MS_Excel.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(ppt|pptx|pps))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/MS_PowerPoint.png";
                    }
                    else if (Regex.Match(fileName, @"$(?<=\.(zip|rar|7z|tar.gz))", RegexOptions.IgnoreCase).Success)
                    {
                        return "../wwwroot/images/learning_material/ZIP.png";
                    }
                    else
                    {
                        return "../wwwroot/images/learning_material/File.png";
                    }
                }
                return "../wwwroot/images/learning_material/File.png";
            }
            catch
            {
                return "../wwwroot/images/learning_material/File.png";
            }
        }

        public enum LoginStatus : int
        {
            Waiting = -1,
            LoggedOff = 0,
            LoggedIn = 1

        };

        public enum ModuleNumber : int
        {
            LearningMaterials = 0,
            MediaLibrary = 1,
            SharedDocuments = 2
        };

 public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }


        // Recurses down the folder structure
        //
        public static void CompressFolder(string path, ZipOutputStream zipStream, int folderOffset)
        {

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressFolder(folder, zipStream, folderOffset);
            }
        }
        public static string convertBooltoString(bool value)
        {
            return value.ToString(); 
        }
    }
}
