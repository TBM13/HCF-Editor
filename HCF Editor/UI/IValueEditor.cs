namespace HCF_Editor.UI
{
    public interface IValueEditor
    {
        public delegate void ObjectEvent(object obj);

        /// <summary>
        /// Invoked when the value is edited and saved.
        /// </summary>
        public ObjectEvent? OnValueEdited { get; set; }


        /// <summary>
        /// Gets/sets the content of the Label.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The object that will be edited. Must not be null.
        /// </summary>
        public object? Value { get; set; }
    }
}