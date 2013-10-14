using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Samson.Core.MinistryPlatform;
using System.Configuration;
using System.Globalization;
using System.Reflection;
using System.ComponentModel;
using System.Web;
using Samson.Models;
using System.Drawing;

namespace Samson.Core
{
    public class MinistryPlatformDataContext
    {

        /// <summary>
        ///     Name of the setting storing system user password in application
        ///     configuration file.
        /// </summary>
        private const string _passwordSettingName = "mppw";
        private const string _domainSettingName = "mpguid";
        private const string _serverSettingName = "mpserver";
        private readonly string _userName;
        private readonly string _password;
        private readonly string _serverName;


        private apiSoapClient _client;


        /// <summary>
        /// Constructor for MinistryPlatformDataContext. Assumes that it can find application settings with key values of "mppw" for password,
        /// "mpguid" for domain guid, and "mpserver" for server name
        /// </summary>
        public MinistryPlatformDataContext()
        {
            var userName = ConfigurationManager.AppSettings[_domainSettingName];
            var pw = ConfigurationManager.AppSettings[_passwordSettingName];
            var serverName = ConfigurationManager.AppSettings[_serverSettingName];

            if (!String.IsNullOrEmpty(pw) && !String.IsNullOrEmpty(userName) && !String.IsNullOrEmpty(serverName))
            {
                _userName = userName;
                _password = pw;
                _serverName = serverName;

                if (HttpContext.Current != null)
                    _client = ObjectFactory.GetWebRequestScoped<apiSoapClient>();
                else
                    _client = ObjectFactory.GetThreadScoped<apiSoapClient>();

                return;
            }

            if (String.IsNullOrEmpty(userName))
                throw new ConfigurationErrorsException("Setting not found - " + _domainSettingName);

            if (String.IsNullOrEmpty(pw))
                throw new ConfigurationErrorsException("Setting not found - " + _passwordSettingName);

            if (String.IsNullOrEmpty(pw))
                throw new ConfigurationErrorsException("Setting not found - " + _serverSettingName);

        }


        /// <summary>
        /// Constructor for MinistryPlatformDataContext. Explicitly provide the credentials and server name for the API.
        /// </summary>
        /// <param name="domainGUID">The domain guid for the domain you want to operate against in Ministry Platform</param>
        /// <param name="apiPassword">The api password</param>
        /// <param name="serverName">The server name of where the api resides</param>
        public MinistryPlatformDataContext(string domainGUID, string apiPassword, string serverName)
        {
            if (!String.IsNullOrEmpty(apiPassword) && !String.IsNullOrEmpty(domainGUID) && !String.IsNullOrEmpty(serverName))
            {
                _userName = domainGUID;
                _password = apiPassword;
                _serverName = serverName;

                if (HttpContext.Current != null)
                    _client = ObjectFactory.GetWebRequestScoped<apiSoapClient>();
                else
                    _client = ObjectFactory.GetThreadScoped<apiSoapClient>();

                return;
            }

            if (String.IsNullOrEmpty(domainGUID))
                throw new ConfigurationErrorsException("Setting not found - " + _domainSettingName);

            if (String.IsNullOrEmpty(apiPassword))
                throw new ConfigurationErrorsException("Setting not found - " + _passwordSettingName);

        }

        /// <summary>
        /// Authenticates a user with Ministry Platform.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="userInfo">The returned user info</param>
        /// <returns></returns>
        public bool AuthenticateUser(string username, string password, out AuthenticatedUserInfo userInfo)
        {
            int userID = -1, contactID = -1, domainID = -1;
            string domainGUID, userGUID, displayName, contactEmail, externalUrl = string.Empty;
            bool canImpersonate = false;

            _client.AuthenticateUser(username, password, _serverName, out userID, out contactID, out domainID, out domainGUID, out userGUID, out displayName, out contactEmail, out externalUrl, out canImpersonate);

            userInfo = new AuthenticatedUserInfo
            {
                CanImpersonate = canImpersonate,
                ContactEmail = contactEmail,
                ContactID = contactID,
                DisplayName = displayName,
                DomainGuid = new Guid(domainGUID),
                DomainID = domainID,
                ExternalUrl = externalUrl,
                UserGuid = new Guid(userGUID),
                UserID = userID
            };

            if (userID > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Attaches an image to the specified record in Ministry Platform
        /// </summary>
        /// <param name="image"></param>
        /// <param name="filename"></param>
        /// <param name="fileDescription"></param>
        /// <param name="pageID"></param>
        /// <param name="recordID"></param>
        /// <param name="resizeLongestSideInPixels"></param>
        public void AttachImage(Image image, string filename, string fileDescription, int pageID, int recordID, int resizeLongestSideInPixels)
        {
            string response = _client.AttachFile(_userName, _password, image.ToByteArray(), filename, pageID, recordID, fileDescription, true, resizeLongestSideInPixels);
            HandleResponse(response.Split('|'), "attaching a file");
        }

        /// <summary>
        /// Attaches a file to the specified record in Ministry Platform. Assumes it's not an image.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="filename"></param>
        /// <param name="fileDescription"></param>
        /// <param name="pageID"></param>
        /// <param name="recordID"></param>
        public void AttachFile(byte[] file, string filename, string fileDescription, int pageID, int recordID)
        {
            string response = _client.AttachFile(_userName, _password, file, filename, pageID, recordID, fileDescription, false, 0);
            HandleResponse(response.Split('|'), "attaching a file");
        }

        /// <summary>
        /// Creates a new record in Ministry Platform
        /// </summary>
        /// <typeparam name="T">A type that inherits from the BaseModel class</typeparam>
        /// <param name="itemToInsert">The object to insert</param>
        /// <returns></returns>
        public T Create<T>(T itemToInsert)
            where T : BaseModel, new()
        {
            return Create<T>(itemToInsert, 0);
        }

        /// <summary>
        /// Creates a new record in Ministry Platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToInsert"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public T Create<T>(T itemToInsert, int UserID)
            where T : BaseModel, new()
        {
            string primaryKey, tableName = string.Empty;
            GetTableInfo(itemToInsert, out primaryKey, out tableName);
            return Create<T>(itemToInsert, UserID, primaryKey, tableName);
        }

        /// <summary>
        /// Creates a new record in Ministry Platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToInsert"></param>
        /// <param name="UserID"></param>
        /// <param name="primaryKey"></param>
        /// <returns></returns>
        public T Create<T>(T itemToInsert, int UserID, string primaryKey)
            where T : BaseModel, new()
        {
            return Create<T>(itemToInsert, UserID, primaryKey, GetTableName(itemToInsert));
        }

        /// <summary>
        /// Creates a new record in Ministry Platform
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="itemToInsert"></param>
        /// <param name="UserID"></param>
        /// <param name="primaryKey"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public T Create<T>(T itemToInsert, int UserID, string primaryKey, string tableName)
            where T : BaseModel, new()
        {
            string[] response = _client.AddRecord(_userName, _password, UserID, tableName, primaryKey, SerializeForAPI(itemToInsert)).Split('|');
            HandleResponse(response, "adding a record");

            //set the id that's been returned to the current object
            Type t = itemToInsert.GetType();
            foreach (var propInfo in t.GetProperties())
            {
                if (propInfo.CustomAttributes.Any(f => f.AttributeType == typeof(KeyAttribute)))
                {
                    propInfo.SetValue(itemToInsert, Int32.Parse(response[0]), null);
                    return itemToInsert;
                }
            }

            return itemToInsert;
        }

        //public dynamic Create(dynamic itemToInsert, string primaryKey, string tableName)
        //{
        //    return Create(itemToInsert, 0, primaryKey, tableName);
        //}

        //public dynamic Create(dynamic itemToInsert, int userID, string primaryKey, string tableName)
        //{
        //    string[] response = _client.AddRecord(_userName, _password, userID, primaryKey, tableName, SerializeForAPI(itemToInsert)).Split('|');
        //    HandleResponse(response, "adding a record");

        //    //set the id that's been returned to the current object
        //    IEnumerable<KeyValuePair<string, object>> obj = (IEnumerable<KeyValuePair<string, object>>) itemToInsert;
        //    foreach (var property in obj)
        //    {
        //        if (property.Key.CompareTo(primaryKey) == 0)
        //        {
        //            itemToInsert[property.Key] = Int32.Parse(response[0]);
        //            return itemToInsert;
        //        }
        //    }

        //    return itemToInsert;
        //}


        public void Update<T>(T itemToUpdate)
            where T : BaseModel, new()
        {
            Update<T>(itemToUpdate, 0);
        }

        public void Update<T>(T itemToUpdate, int UserID)
            where T : BaseModel, new()
        {

            string primaryKey, tableName = string.Empty;
            GetTableInfo(itemToUpdate, out primaryKey, out tableName);
            Update<T>(itemToUpdate, UserID, primaryKey, tableName);
        }

        public void Update<T>(T itemToUpdate, int UserID, string primaryKey)
            where T : BaseModel, new()
        {
            Update<T>(itemToUpdate, UserID, primaryKey, GetTableName(itemToUpdate));
        }

        public void Update<T>(T fieldsToUpdate, int UserID, string primaryKey, string tableName)
            where T : BaseModel, new()
        {
            string[] response = _client.UpdateRecord(_userName, _password, UserID, tableName, primaryKey, SerializeForAPIOnUpdate(primaryKey, fieldsToUpdate)).Split('|');
            HandleResponse(response, "updating a record");
        }

        public void Update<T>(T oldItem, T newItem)
    where T : BaseModel, new()
        {
            Update<T>(oldItem, newItem, 0);
        }

        public void Update<T>(T oldItem, T newItem, int UserID)
            where T : BaseModel, new()
        {

            string primaryKey, tableName = string.Empty;
            GetTableInfo(newItem, out primaryKey, out tableName);
            Update<T>(oldItem, newItem, UserID, primaryKey, tableName);
        }

        public void Update<T>(T oldItem, T newItem, int UserID, string primaryKey)
            where T : BaseModel, new()
        {
            Update<T>(oldItem, newItem, UserID, primaryKey, GetTableName(newItem));
        }

        public void Update<T>(T oldItem, T newItem, int UserID, string primaryKey, string tableName)
            where T : BaseModel, new()
        {
            string[] response = _client.UpdateRecord(_userName, _password, UserID, tableName, primaryKey, SerializeForAPIOnUpdate(primaryKey, oldItem, newItem)).Split('|');
            HandleResponse(response, "updating a record");
        }

        public IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName)
            where T : BaseModel, new()
        {
            return ExecuteStoredProcedure<T>(storedProcedureName, null);
        }

        public IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName, object requestObject)
            where T : BaseModel, new()
        {
            var data = _client.ExecuteStoredProcedure(_userName, _password, storedProcedureName, requestObject != null ? (string)SerializeForAPI(requestObject) : string.Empty);

            if (data.Tables.Count < 0)
                return null;
            return data.Tables[0].ToList<T>();
        }

        //public IEnumerable<T> ExecuteStoredProcedure<T>(string storedProcedureName, dynamic requestObject)
        //    where T : new()
        //{
        //    var data = _client.ExecuteStoredProcedure(_userName, _password, storedProcedureName, requestObject != null ? (String)SerializeForAPI(requestObject) : string.Empty);
        //    if (data.Tables.Count < 0)
        //        return null;
        //    return data.Tables[0].ToList<T>();
        //}

        public GridReader ExecuteStoredProcedureMultiSet<S>(string storedProcedureName, S requestObject)
        {
            return new GridReader(_client.ExecuteStoredProcedure(_userName, _password, storedProcedureName, SerializeForAPI(requestObject)));
        }

        public GridReader ExecuteStoredProcedureMultiSet(string storedProcedureName)
        {
            return new GridReader(_client.ExecuteStoredProcedure(_userName, _password, storedProcedureName, string.Empty));
        }

        private void HandleResponse(string[] response, string method)
        {
            int mqID = Int32.Parse(response[0]);
            if (mqID == 0) //if Ministry Platform returns an error
                throw new Exception("An error occured " + method + ". " + response[2]);
        }

        private string GetTableName(object itemToInsert)
        {
            //Looks for the table attribute on the object's type. if it's there, then
            //we used the specified name. Else use the type name for the DB.
            TableAttribute tblattr = itemToInsert.GetType().GetCustomAttribute<TableAttribute>();
            if (tblattr != null)
                return tblattr.Name;
            else
                return itemToInsert.GetType().Name;
        }

        private void GetTableInfo(object item, out string primaryKey, out string tableName)
        {
            tableName = GetTableName(item);

            //look for the primary key attribute for each property of the specified type
            foreach (var property in item.GetType().GetProperties())
            {
                KeyAttribute keyAttribute = property.GetCustomAttribute<KeyAttribute>();

                if (keyAttribute != null)
                {
                    //look to see if this has a column attribute
                    ColumnAttribute colAttr = property.GetCustomAttribute<ColumnAttribute>();
                    if (colAttr != null)
                        primaryKey = colAttr.Name;
                    else
                        primaryKey = property.Name;

                    return;
                }
            }

            primaryKey = string.Empty;
            throw new ConfigurationErrorsException("No Primary Key defined for the type \"" + item.GetType().Name + "\". Define a primary key for this type to use the Ministry Platform API");
        }

        /// <summary>
        /// This method serializes an object based on what's in it's 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKey"></param>
        /// <param name="item"></param>
        /// <param name="OriginalItem"></param>
        /// <returns></returns>
        private string SerializeForAPIOnUpdate<T>(string primaryKey, T OriginalItem)
            where T : BaseModel, new()
        {
            var j = 0;
            var sb = new StringBuilder();

            sb.Append(primaryKey);
            sb.Append("=");
            sb.Append(OriginalItem.GetType().GetProperty(primaryKey).GetValue(OriginalItem).ToString().APIEncode());
            sb.Append("&");

            foreach (var property in OriginalItem.ChangedProperties)
            {
                object value = null;

                ColumnAttribute colAttr = OriginalItem.GetType().GetProperty(property.Key).GetCustomAttribute<ColumnAttribute>();//property.Value.GetType().GetCustomAttribute<ColumnAttribute>();
                if (colAttr != null)
                    sb.Append(colAttr.Name);
                else
                    sb.Append(property.Key);


                //if value of new item is null
                if (property.Value != null)
                {
                    if (property.Value.GetType() == typeof(Boolean))
                        value = ((bool)property.Value).APIEncode();

                    if (value == null)
                        value = property.Value;

                    sb.Append("=").Append(value.ToString().APIEncode());
                }
                else
                    sb.Append("=");

                //only add an ampersand if there's another property to add
                if (++j < OriginalItem.ChangedProperties.Count())
                    sb.Append("&");
            }

            string returnString = sb.ToString();
            if (returnString.EndsWith("&"))
                returnString = returnString.TrimEnd('&');

            return returnString;
        }

        /// <summary>
        /// This version compares two objects and only serializes the difference
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="NewItem"></param>
        /// <param name="OriginalItem"></param>
        /// <returns></returns>
        private string SerializeForAPIOnUpdate<T>(string primaryKey, T OriginalItem, T NewItem)
            where T : BaseModel, new()
        {
            var j = 0;
            var sb = new StringBuilder();

            sb.Append(primaryKey);
            sb.Append("=");
            sb.Append(OriginalItem.GetType().GetProperty(primaryKey).GetValue(OriginalItem).ToString().APIEncode());
            sb.Append("&");
            
            var delta = OriginalItem.DetailedCompare<T>(NewItem);

            foreach (var property in delta)
            {

                object value = null;


                ColumnAttribute colAttr = OriginalItem.GetType().GetProperty(property.Prop).GetCustomAttribute<ColumnAttribute>();//property.Value.GetType().GetCustomAttribute<ColumnAttribute>();
                if (colAttr != null)
                    sb.Append(colAttr.Name);
                else
                    sb.Append(property.Prop);

                //if value of new item is null
                if (property.valB != null)
                {
                    if (property.valB.GetType() == typeof(Boolean)) //valB is the value of the new item
                        value = ((bool)property.valB).APIEncode();

                    if (value == null)
                        value = property.valB;

                    sb.Append("=").Append(value.ToString().APIEncode());
                }
                else
                    sb.Append("=");

                //only add an ampersand if there's another property to add
                if (++j < delta.Count())
                    sb.Append("&");
            }

            string returnString = sb.ToString();
            if (returnString.EndsWith("&"))
                returnString = returnString.TrimEnd('&');

            return returnString;
        }

        private string SerializeForAPI(object item)
        {
            var j = 0;
            //var keys = props.Keys;
            var props = item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var sb = new StringBuilder();
            foreach (var property in props)
            {
                bool isPrimaryKey = false;
                bool isBrowsable = true; //assume all properties are fair game...


                //look for primary key - we'll skip this later
                KeyAttribute attr = property.GetCustomAttribute<KeyAttribute>();
                if (attr != null)
                    isPrimaryKey = true;
                
                //look for not browsable attr - we'll skip this later as well
                BrowsableAttribute battr = property.GetCustomAttribute<BrowsableAttribute>();
                if (battr != null)                    
                    isBrowsable = battr.Browsable;

                var value = property.GetValue(item, null);

                //if this property doesn't have anything in it, or it's a primary key, or it's not browsable, skip it.
                if (value != null && value.ToString() != string.Empty && !isPrimaryKey && isBrowsable)
                {
                    if (value.GetType() == typeof(Boolean))
                        value = ((bool)value).APIEncode();

                    ColumnAttribute colAttr = property.GetCustomAttribute<ColumnAttribute>();
                    if (colAttr != null)
                        sb.Append(colAttr.Name);
                    else
                        sb.Append(property.Name);

                    sb.Append("=").Append(value.ToString().APIEncode());

                    //only add an ampersand if there's another property to add
                    if (++j < props.Count())
                        sb.Append("&");
                }
                else
                    j++;


            }

            string returnString = sb.ToString();
            if (returnString.EndsWith("&"))
                returnString = returnString.TrimEnd('&');

            return returnString;
        }

        //private string SerializeForAPI(object item, string primaryKey = null, bool isUpdate = false)
        //{

        //    return SerializeForAPI(item);
        //    //var j = 0;
        //    ////var keys = props.Keys;
        //    //var sb = new StringBuilder();
        //    //IEnumerable<KeyValuePair<string, object>> items = (IEnumerable<KeyValuePair<string, object>>)item;
        //    //foreach (var property in items)
        //    //{
        //    //    if (property.Key.CompareTo(primaryKey) == 0 && !isUpdate)
        //    //        continue; //skip over this one - it's the primary key

        //    //    //if this property doesn't have anything in it, or it's a primary key, or it's not browsable, skip it.
        //    //    if (property.Value != null && property.Value.ToString() != string.Empty)
        //    //    {
        //    //        if (property.Value.GetType() == typeof(Boolean))
        //    //            sb.Append(property.Key).Append("=").Append(((bool)property.Value).APIEncode());
        //    //        sb.Append(property.Key).Append("=").Append(property.Value.ToString().APIEncode());

        //    //        //only add an ampersand if there's another property to add
        //    //        if (++j < items.Count())
        //    //            sb.Append("&");
        //    //    }
        //    //}

        //    //return sb.ToString();
        //}
    }

    public class GridReader : IDisposable
    {
        private readonly DataSet dSet;
        private int counter = 0;

        public GridReader(DataSet initial)
        {
            dSet = initial;
        }

        public IEnumerable<T> Read<T>()
            where T : BaseModel, new()
        {

            IEnumerable<T> list = dSet.Tables[counter].ToList<T>();
            counter++;
            return list;
        }

        public void Dispose()
        {
            dSet.Dispose();
        }
    }
}
