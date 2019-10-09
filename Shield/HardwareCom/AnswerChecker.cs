using Shield.Enums;

namespace Shield.HardwareCom
{
    public class AnswerChecker
    {
        private const int Correct = 1;
        private const int Problem = 0;
        private const int Error = -1;

        public int Check(CommandType sended, CommandType received)
        {
            int firstCheck = IsTypeCorrect(sended, received);
            return 1;
        }

        private int IsTypeCorrect(CommandType sended, CommandType received)
        {
            switch (sended)
            {
                case CommandType.HandShake:
                    if(received == CommandType.HandShake)
                        return Correct;
                    else 
                        break;

                case CommandType.Confirm:
                    if(received == CommandType.Correct)
                        return Correct;
                    else
                        break;



            }
            return Problem;
        }
    }
}