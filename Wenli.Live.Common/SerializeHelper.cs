using Newtonsoft.Json;
using ProtoBuf;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Wenli.Live.Common
{
    public class SerializeHelper
    {
        /// <summary>
        ///     将C#数据实体转化为xml数据
        /// </summary>
        /// <param name="obj">要转化的数据实体</param>
        /// <returns>xml格式字符串</returns>
        public static string XmlSerialize<T>(T obj)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            var stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;

            var sr = new StreamReader(stream);
            var resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        /// <summary>
        ///     将xml数据转化为C#数据实体
        /// </summary>
        /// <param name="json">符合xml格式的字符串</param>
        /// <returns>T类型的对象</returns>
        public static T XmlDeserialize<T>(string xml)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml.ToCharArray()));
            var obj = (T)serializer.ReadObject(ms);
            ms.Close();

            return obj;
        }
        /// <summary>
        ///     二进制序列化
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] ByteSerialize(object obj)
        {
            using (var m = new MemoryStream())
            {
                var bin = new BinaryFormatter();

                bin.Serialize(m, obj);

                return m.ToArray();
            }
        }

        /// <summary>
        ///     二进制反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ByteDeserialize<T>(byte[] buffer)
        {
            using (var m = new MemoryStream())
            {
                m.Write(buffer, 0, buffer.Length);
                m.Position = 0;

                var bin = new BinaryFormatter();

                return (T)bin.Deserialize(m);
            }
        }

        /// <summary>
        ///     newton.json序列化,日志参数专用
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            return JsonConvert.SerializeObject(obj, settings);
        }

        /// <summary>
        ///     newton.json反序列化,日志参数专用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            settings.DateFormatString = "yyyy-MM-dd HH:mm:ss.fff";
            return JsonConvert.DeserializeObject<T>(json, settings);
        }


        /// <summary>
        ///     ProtolBuf序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static byte[] ProtolBufSerialize<T>(T instance)
        {
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, instance);
                ms.Flush();
                return ms.ToArray();
            }
        }

        /// <summary>
        ///     ProtolBuf反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T ProtolBufDeserialize<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                return Serializer.Deserialize<T>(ms);
            }
        }
    }
}
