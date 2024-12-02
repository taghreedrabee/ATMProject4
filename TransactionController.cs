using ATMAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Reflection.PortableExecutable;
using System.Security.Claims;

namespace ATMAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly PendingTransfer _pendingTransfer;
        private User _user;

        public TransactionController(UserRepository userRepository, TransactionRepository transactionRepository, PendingTransfer pendingTransfer )
        {
            _pendingTransfer = pendingTransfer;
            _userRepository = userRepository;   
            _transactionRepository = transactionRepository;
        }

        [HttpGet("CheckBalance")]
        public IActionResult CheckBalance([FromQuery] int userid)
        {
            var user = _userRepository.GetUserById(userid);
            if (user == null) {
                return NotFound("user not found" );
                
            }
            else
            {
              

                double balance = user.Balance;
                _transactionRepository.AddTransaction(new TransactionInfo(
                    0,
                    user.UserId,
                    user.Username,
                    "Balance Check",
                    0,
                    DateTime.Now,
                    0,
                    0,
                    null,
                    true,
                    "Financial"
                ));
                return Ok($"the balance of user {user.Username} is {user.Balance}");

            }

        }

        [HttpPost("Deposite")]
        public IActionResult Deposite(int userid , double amount)
        {
            var User = _userRepository.GetUserById(userid);
            if (amount > 0) {
                double oldBalance = User.Balance;
                User.Balance += amount;
                _userRepository.UpdateUser(User);

                

                var transaction = new TransactionInfo(
                    0,
                    User.UserId,
                    User.Username,
                    "Deposit",
                    amount,
                    DateTime.Now,
                    oldBalance,
                     User.Balance,
                    null,
                    true,
                    "Financial"
                );
                _transactionRepository.AddTransaction(transaction);
               return Ok($"amount added successfully your balance is {User.Balance}");

            }
            else
            {
               return BadRequest("Invalid amount");
            }


        }

        [HttpPost("Withdraw")]
        public IActionResult Withdraw(int userid, double amount) { 
            var user = _userRepository.GetUserById(userid);
            if (amount > 0 && amount < user.Balance) {
                double oldBalance = user.Balance;
                user.Balance -= amount;

                _userRepository.UpdateUser(user);

                var transaction = new TransactionInfo(
                    0,
                    user.UserId,
                    user.Username,
                    "Withdrawal",
                    amount,
                    DateTime.Now,
                    oldBalance,
                    user.Balance,
                    null,
                    true,
                    "Financial"
                );
                _transactionRepository.AddTransaction(transaction);
                return Ok($"amount added successfully your balance is {user.Balance}");

            }
               return BadRequest("Invalid amount");
        }

        [HttpPost("TransfarMoney")]
        public IActionResult Transfermoney(int userid, int recipientid,double amount)
        {
            var sender = _userRepository.GetUserById(userid);
            var reciever = _userRepository.GetUserById(recipientid);
            if (sender != null && recipientid != null) {

                if (amount > 0 && amount < sender.Balance)
                {
                    double senderOldBalance = sender.Balance;
                    sender.Balance -= amount;
                    _userRepository.UpdateUser(sender);

                    var pendingTransfer = new PendingTransfer(
                        0,
                        sender.UserId,
                        reciever.Username,
                        reciever.UserId,
                        amount
                    );

                    _pendingTransfer.AddPendingTransfer(pendingTransfer);

                    var senderTransaction = new TransactionInfo(
                        0,
                        sender.UserId,
                        sender.Username,
                        "Transfer (Sent - Pending)",
                        amount,
                        DateTime.Now,
                        senderOldBalance,
                        sender.Balance,
                        reciever.UserId,
                        false,
                        "Financial"
                    );
                    _transactionRepository.AddTransaction(senderTransaction);
                    return Ok($"Transfer initiated. Amount {amount:C} is pending recipient's approval. Your new balance is: {sender.Balance:C}");
                }
                else
                {
                    return BadRequest("Invalid amount or insufficient balance.");
                }

            }
            else
            {
                return NotFound("Sender or recipient not found. ");
            }

        }

        [HttpGet("AllHistory")]
        public IActionResult History([FromQuery] int userid)
        {
            var user = _userRepository.GetUserById(userid);
            if(user != null)
            {
                var transactions = _transactionRepository.GetTransactionsByUserId(user.UserId)
                 .OrderByDescending(t => t.DateTime);

                _transactionRepository.AddTransaction(new TransactionInfo(
                0,
                user.UserId,
                user.Username,
                $"View Transactions",
                0,
                DateTime.Now,
                0,
                0,
                null,
                true,
                "Financial"
            ));
               return Ok(transactions);
            }
            else
            {
               return NotFound("user not found");
            }
        }


        [HttpGet("FinancialHistory")]
        public IActionResult FinancialHistory([FromQuery] int userid)
        {
            var user = _userRepository.GetUserById(userid);
            if (user != null)
            {
                var transactions = _transactionRepository.GetFinancialTransactions(user.UserId)
                 .OrderByDescending(t => t.DateTime);

                _transactionRepository.AddTransaction(new TransactionInfo(
                0,
                user.UserId,
                user.Username,
                $"View financial Transactions",
                0,
                DateTime.Now,
                0,
                0,
                null,
                true,
                "Financial"
            ));
                return Ok(transactions);
            }
            else
            {
               return NotFound("user not found");
            }
        }

        [HttpGet("ManageralHistory")]
        public IActionResult ManageralHistory([FromQuery] int userid)
        {
            var user = _userRepository.GetUserById(userid);
            if (user != null)
            {
                var transactions = _transactionRepository.GetManagerialTransactions(user.UserId)
                 .OrderByDescending(t => t.DateTime);

                _transactionRepository.AddTransaction(new TransactionInfo(
                0,
                user.UserId,
                user.Username,
                $"View Managiral Transactions",
                0,
                DateTime.Now,
                0,
                0,
                null,
                true,
                "Financial"
            ));
               return Ok(transactions);
            }
            else
            {
               return NotFound("user not found");
            }
        }


        [HttpGet("PendingTransfer")]
        public IActionResult GetPendingTransfers([FromQuery] int RecipientId)
        {
            
            var pendingTransfers = _pendingTransfer.GetPendingTransfersByRecipientId(RecipientId);
            if(RecipientId == null)
            {
                return NotFound("No Pending Transfers");
            }
            return Ok(pendingTransfers);
        }

        [HttpPost("accept")]
        public IActionResult AcceptTransfer(int transferId , int RecipientId)
        {
            var transfer =  _pendingTransfer.GetPendingTransferById(transferId);

            if (transfer == null && transfer.RecipientId == RecipientId)
            {
                return NotFound("transfer not found");
            }

            var recipient = _userRepository.GetUserById(RecipientId);
            recipient.Balance += transfer.Amount;
            _userRepository.UpdateUser(recipient);

            var recipientTransaction = new TransactionInfo
            {
                UserId = RecipientId,
                Username = recipient.Username,
                TransactionName = "Transfer (Received)",
                Amount = transfer.Amount,
                DateTime = DateTime.Now,
                BalanceBefore = recipient.Balance - transfer.Amount,
                BalanceAfter = recipient.Balance,
                RecipientId = transfer.SenderId,
                IsComplete = true,
                TransactionType = "Financial"
            };

            _transactionRepository.AddTransaction(recipientTransaction);

            _pendingTransfer.DeletePendingTransfer(transferId);

            return Ok(new { message = "Transfer accepted. Your new balance is: " + recipient.Balance.ToString("C") });
        }

        [HttpPost("reject")]
        public IActionResult RejectTransfer(int transferId, int RecipientId)
        {
            var transfer =  _pendingTransfer.GetPendingTransferById(transferId);

            if (transfer == null || transfer.RecipientId != RecipientId)
            {
                return NotFound("transfer not found");
            }

            var sender =  _userRepository.GetUserById(transfer.SenderId);
            sender.Balance += transfer.Amount;
             _userRepository.UpdateUser(sender);

            var refundTransaction = new TransactionInfo
            {
                UserId = sender.UserId,
                Username = sender.Username,
                TransactionName = "Transfer (Refunded)",
                Amount = transfer.Amount,
                DateTime = DateTime.Now,
                BalanceBefore = sender.Balance - transfer.Amount,
                BalanceAfter = sender.Balance,
                RecipientId = RecipientId,
                IsComplete = true,
                TransactionType = "Financial"
            };

             _transactionRepository.AddTransaction(refundTransaction);

             _pendingTransfer.DeletePendingTransfer(transferId);

            return Ok(new { message = $"Transfer rejected. Money has been returned to sender." });
        }

    }
}
