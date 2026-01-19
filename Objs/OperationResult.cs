namespace LibraryManagement.Objs
{
    public class OperationResult<T>
    {
        public string StatusMessage { get; set; }
        public T Data { get; set; }

        public OperationResult() { }

        public OperationResult(string statusMessage, T data)
        {
            StatusMessage = statusMessage;
            Data = data;
        }
    }

}
