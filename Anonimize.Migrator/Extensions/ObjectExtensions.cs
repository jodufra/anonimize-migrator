using System.Linq;

namespace System
{
    public static class ObjectExtensions
    {

        public static bool HasProperty(this object obj, string propertyName)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();
            return properties.Any(p => p.Name.Equals(propertyName));
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var type = obj.GetType();
            var properties = type.GetProperties();
            var property = properties.First(p => p.Name.Equals(propertyName));
            return property.GetValue(obj);
        }

        public static bool IsEncrypted(this object obj)
        {
            if (obj == null || !(obj is string))
                return false;

            var anonimize = Anonimize.AnonimizeProvider.GetInstance();
            var service = anonimize.GetCryptoService();
            var decrypted = service.Decrypt<object>((string)obj);
            return decrypted != null;
        }

    }
}
