using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Samson.Core
{

   public class ObjectFactory
    {
        /// <summary> 
        /// Creates a ASP.NET Context scoped instance of an object. This static
        /// method creates a single instance and reuses it whenever this method is
        /// called.
        /// 
        /// This version creates an internal request specific key shared key that is
        /// shared by each caller of this method from the current Web request.
        /// </summary>
        public static T GetWebRequestScoped<T>()
        {
            // *** Create a request specific unique key 
            return (T)GetWebRequestScopedInternal(typeof(T), null, null);
        }

        /// <summary>
        /// Creates a ASP.NET Context scoped instance of a DataContext. This static
        /// method creates a single instance and reuses it whenever this method is
        /// called.
        /// 
        /// This version lets you specify a specific key so you can create multiple 'shared'
        /// DataContexts explicitly.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetWebRequestScoped<T>(string key)
        {
            return (T)GetWebRequestScopedInternal(typeof(T), key, null);
        }


        public static T GetWebRequestScoped<T>(string key, object[] args)
        {
            return (T)GetWebRequestScopedInternal(typeof(T), key, args);
        }

        /// <summary>
        /// Internal method that handles creating a context that is scoped to the HttpContext Items collection
        /// by creating and holding the DataContext there.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        static object GetWebRequestScopedInternal(Type type, string key, object[] args)
        {
            object context;

            if (HttpContext.Current == null)
            {
                //if 
                if (args == null)
                    context = Activator.CreateInstance(type);
                else
                context = Activator.CreateInstance(type, args);

                return context;
            }

            // *** Create a unique Key for the Web Request/Context 
            if (key == null)
                key = "__WRSCDC_" + HttpContext.Current.GetHashCode().ToString("x") + Thread.CurrentContext.ContextID.ToString();

            context = HttpContext.Current.Items[key];
            if (context == null)
            {
                if (args == null)
                    context = Activator.CreateInstance(type);
                else
                    context = Activator.CreateInstance(type, args);

                if (context != null)
                    HttpContext.Current.Items[key] = context;
            }

            return context;
        }

        /// <summary>
        /// Creates a Thread Scoped DataContext object that can be reused.
        /// The DataContext is stored in Thread local storage.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetThreadScoped<T>()
        {
            return (T)GetThreadScopedInternal(typeof(T), null, null);
        }


        /// <summary>
        /// Creates a Thread Scoped DataContext object that can be reused.
        /// The DataContext is stored in Thread local storage.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetThreadScoped<T>(string key)
        {
            return (T)GetThreadScopedInternal(typeof(T), key, null);
        }

        public static T GetThreadScoped<T>(string key, object[] args)
        {
            return (T)GetThreadScopedInternal(typeof(T), key, args);
        }

        /// <summary>
        /// Creates a Thread Scoped DataContext object that can be reused.
        /// The DataContext is stored in Thread local storage.
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        static object GetThreadScopedInternal(Type type, string key, object[] args)
        {
            if (key == null)
                key = "__WRSCDC_" + Thread.CurrentContext.ContextID.ToString();

            LocalDataStoreSlot threadData = Thread.GetNamedDataSlot(key);
            object context = null;
            if (threadData != null)
                context = Thread.GetData(threadData);

            if (context == null)
            {
                if (args == null)
                    context = Activator.CreateInstance(type);
                else
                    context = Activator.CreateInstance(type, args);

                if (context != null)
                {
                    if (threadData == null)
                        threadData = Thread.AllocateNamedDataSlot(key);

                    Thread.SetData(threadData, context);
                }
            }

            return context;
        }
    }
}
