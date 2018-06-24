namespace Plugins.ModLab
{
    public class ModLabObjectChangedEvent
    {
        private object _original, _current;
        
        public ModLabObjectChangedEvent(object o, ref object changeable)
        {
            _original = o;
            _current = changeable;
        }
    }
}