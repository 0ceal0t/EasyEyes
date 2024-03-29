using EasyEyes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VFXSelect.Data.Sheets {
    public abstract class SheetLoader<T, S> { // T = Not Selected, S = Selected
        public List<T> Items = new();
        public bool Loaded = false;
        public bool Waiting = false;

        public abstract void OnLoad();
        public abstract bool SelectItem( T item, out S selectedItem );

        public void Load() {
            if( Waiting ) return;
            Waiting = true;
            Services.Log( "Loading " + typeof( T ).Name );
            Task.Run( async () => {
                try {
                    OnLoad();
                }
                catch( Exception e ) {
                    Services.Error( e, "Error Loading " + typeof( T ).Name );
                }
                Loaded = true;
            } );
        }
    }
}
