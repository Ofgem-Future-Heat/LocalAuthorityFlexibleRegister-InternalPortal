namespace Ofgem_Web_LAF_InternalPortal
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public bool IsFailure { get; private set; }
        public T? Value { get; private set; }
        public string[]? Errors { get; private set; }

        private Result() { }

        public static Result<T> Success(T value) => new Result<T>() { IsSuccess = true, IsFailure = false, Value = value };
        public static Result<T> Failure(params string[] errors) => new Result<T>() { IsSuccess = false, IsFailure = true, Errors = errors };
    }
}
