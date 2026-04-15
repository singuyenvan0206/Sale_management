namespace FashionStore.Core.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string ErrorMessage { get; }

        protected Result(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true, string.Empty);
        public static Result Failure(string message) => new Result(false, message);
    }

    public class Result<T> : Result
    {
        private readonly T? _value;

        public T Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException("Cannot access value of a failed result.");
                return _value!;
            }
        }

        protected Result(T? value, bool isSuccess, string errorMessage) 
            : base(isSuccess, errorMessage)
        {
            _value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
        public static new Result<T> Failure(string message) => new Result<T>(default, false, message);
    }
}
