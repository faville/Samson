using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samson.Models
{
    public abstract class BaseModel : INotifyPropertyChanged
    {
        private Dictionary<string, object> changedProps;
        private bool trackChanges = false;

        [Browsable(false)]
        public Dictionary<string, object> ChangedProperties {
            get
            {
                return changedProps;
            }
            set
            {
                changedProps = value;
            }
        }

        [Browsable(false)]
        public bool IsTrackingChanges {
            get
            {
                return trackChanges;
            }
            set
            {
                trackChanges = value;
            }
        }
        /// <summary>
        /// Initializes a new instance of the BaseModel class.
        /// </summary>
        protected BaseModel()
        {
            changedProps = new Dictionary<string, object>();
        }

        /// <summary>
        /// Fired when a property in this class changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void BeginTrackingChanges()
        {
            trackChanges = true;
        }
        /// <summary>
        /// Triggers the property changed event for a specific property.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        public void NotifyPropertyChanged(string propertyName, object value)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            if (!changedProps.ContainsKey(propertyName))
            {
                changedProps.Add(propertyName, value);
                return;
            }

            changedProps.Remove(propertyName);
            changedProps.Add(propertyName, value);
            //if(changed
            //changedProps.Add(new KeyValuePair<string,object>(propertyName, value));
        }
    }
}
