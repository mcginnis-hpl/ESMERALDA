namespace ESMERALDAClasses
{
    using System;

    public abstract class Ordered<T>
    {
        protected Ordered()
        {
        }

        public abstract bool Less(Ordered<T> that);
    }
}

