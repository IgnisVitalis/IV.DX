namespace IV.DX.Kernel.Models
{
    public class DXMultiElementsContainer<T> where T : DXElement
    {
        public DXMultiElementsContainer()
        {
            this.Mode = MultiElementsMode.Target;
        }

        public MultiElementsMode Mode { get; set; }
        private HashSet<T>? _announced;
        public HashSet<T> Announced
        {
            get
            {
                if (this._announced == null)
                    this._announced = new HashSet<T>();

                return this._announced;
            }
            set
            {
                this._announced = value;
            }
        }

        private HashSet<T>? _deleted;

        public HashSet<T> Deleted
        {
            get
            {
                if (this._deleted == null)
                    this._deleted = new HashSet<T>();

                return this._deleted;
            }
            set
            {
                this._deleted = value;
            }
        }

        public void AddToAnnounced(T item)
        {
            this.Announced.Add(item);
        }

        public void RemoveFromAnnounced(T item)
        {
            this.Announced = this.Announced.Where(x => !x.Equals(item)).ToHashSet();
        }

        public void AddToDeleted(T item)
        {
            this.Deleted.Add(item);
        }

        public void RemoveFromDeleted(T item)
        {
            this.Deleted = this.Deleted.Where(x => !x.Equals(item)).ToHashSet();
        }
    }
}
