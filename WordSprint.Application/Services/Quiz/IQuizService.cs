using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WordSprint.Application.Models.Quiz;    // şimdilik böyle; ileride DTO'ları Application'a taşırız
public interface IQuizService
{
    Task<SubmitQuizResponse> SubmitAsync(string userId, SubmitQuizRequest request);
}