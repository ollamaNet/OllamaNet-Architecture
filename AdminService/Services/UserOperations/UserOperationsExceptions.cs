using System;

namespace AdminService.Services.UserOperations.Exceptions
{
    /// <summary>
    /// Base exception for all UserOperations exceptions
    /// </summary>
    public class UserOperationsException : Exception
    {
        public UserOperationsException(string message) : base(message) { }
        public UserOperationsException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a user is not found
    /// </summary>
    public class UserNotFoundException : UserOperationsException
    {
        public string UserId { get; }
        
        public UserNotFoundException(string userId) 
            : base($"User with ID '{userId}' was not found")
        {
            UserId = userId;
        }
        
        public UserNotFoundException(string email, bool isEmail) 
            : base($"User with email '{email}' was not found")
        {
            UserId = email;
        }
    }

    /// <summary>
    /// Exception thrown when a user with the same identifier already exists
    /// </summary>
    public class UserAlreadyExistsException : UserOperationsException
    {
        public string Identifier { get; }
        
        public UserAlreadyExistsException(string email)
            : base($"User with email '{email}' already exists")
        {
            Identifier = email;
        }
        
        public UserAlreadyExistsException(string username, bool isUsername)
            : base($"User with username '{username}' already exists")
        {
            Identifier = username;
        }
    }

    /// <summary>
    /// Exception thrown when an invalid operation is attempted on a user
    /// </summary>
    public class InvalidUserOperationException : UserOperationsException
    {
        public string UserId { get; }
        public string Operation { get; }
        
        public InvalidUserOperationException(string userId, string operation)
            : base($"Invalid operation '{operation}' attempted on user '{userId}'")
        {
            UserId = userId;
            Operation = operation;
        }
        
        public InvalidUserOperationException(string userId, string operation, string reason)
            : base($"Invalid operation '{operation}' attempted on user '{userId}': {reason}")
        {
            UserId = userId;
            Operation = operation;
        }
    }
} 