namespace Testproject.Controllers
{
    public abstract class BaseClass
    {
        // Virtual method in the base class
        public virtual void DisplayMessage()
        {
            Console.WriteLine("Message from BaseClass.");
        }
        public abstract void DisplayMeessage();
    }

    public class DerivedClass : BaseClass
    {
        public override void DisplayMeessage()
        {
            throw new NotImplementedException();
        }

        // Overriding the virtual method in the derived class
        public override void DisplayMessage()
        {
            Console.WriteLine("Message from DerivedClass.");
        }
    }
}
