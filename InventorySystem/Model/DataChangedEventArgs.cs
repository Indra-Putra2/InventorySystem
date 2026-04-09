namespace InventorySystem.Model
{
    public class DataChangedEventArgs
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public int Affected { get; set; }
        public bool Notify { get; set; } = true;
    }
}
