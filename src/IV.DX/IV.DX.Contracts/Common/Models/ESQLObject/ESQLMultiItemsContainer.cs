using System.Collections.Generic;
using System.Linq;

namespace IV.DataProvider.Persistence.Contracts.Models
{
    public class ESQLMultiItemsContainer<T> where T : ESQLBlock
    {
        public ESQLMultiItemsContainer()
        {
            this.Mode = ModeForMultiItems.Full;
        }

        public ModeForMultiItems Mode { get; set; }
        private IEnumerable<T> _announced;
        public IEnumerable<T> Announced
        {
            get
            {
                if (this._announced == null)
                    this._announced = new List<T>();

                return this._announced;
            }
            set
            {
                this._announced = value;
            }
        }

        private IEnumerable<T> _deleted;

        public IEnumerable<T> Deleted
        {
            get
            {
                if (this._deleted == null)
                    this._deleted = new List<T>();

                return this._deleted;
            }
            set
            {
                this._deleted = value;
            }
        }

        public void AddToAnnounced(T item)
        {
            this.Announced = this.Announced.Append(item);
        }

        public void RemoveFromAnnounced(T item)
        {
            this.Announced = this.Announced.Where(x => !x.Equals(item)).ToList();
        }

        public void AddToDeleted(T item)
        {
            this.Deleted = this.Deleted.Append(item);
        }

        public void RemoveFromDeleted(T item)
        {
            this.Deleted = this.Deleted.Where(x => !x.Equals(item)).ToList();
        }
    }
}