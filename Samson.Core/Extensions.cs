using Samson.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Samson.Core
{
    public static class Extensions
    {
        public static string APIEncode(this string toEncode)
        {
            if (string.IsNullOrEmpty(toEncode))
                return "";

            return toEncode.Replace("#", "dp_Pound")
                               .Replace("&", "dp_Amp")
                               .Replace("=", "dp_Equal")
                               .Replace("?", "dp_Qmark")
                               .Trim();
        }

        public static string APIEncode(this bool toEncode)
        {
            return toEncode ? "1" : "0";
        }

        public static byte[] ToByteArray(this System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            return ms.ToArray();
        }


        public static List<Varience> DetailedCompare<T>(this T val1, T val2)
        {
            List<Varience> variences = new List<Varience>();
            PropertyInfo[] properties = val1.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                BrowsableAttribute battr = property.GetCustomAttribute<BrowsableAttribute>();
                if (battr != null)                    
                {
                    if(!battr.Browsable)
                        continue;
                }

                KeyAttribute kattr = property.GetCustomAttribute<KeyAttribute>();
                if (kattr != null)
                    continue;

                Varience v = new Varience();
                v.Prop = property.Name;
                v.valA = property.GetValue(val1);
                v.valB = property.GetValue(val2);
                if (v.valA == null && v.valB == null)
                    continue;
                if ((v.valA != null && v.valB == null) || (v.valA == null && v.valB != null))
                {
                    variences.Add(v);
                    continue;
                }
                if (!v.valA.Equals(v.valB))
                    variences.Add(v);

            }
            return variences;
        }
    }
}
