namespace MyNamespace
{
    public class MyClass
    {
        private int myField;
        public int MyProperty 
        {
            get 
            {
                return myField;
            }
            set 
            {
                myField = value;
            }
        }

        public event Type MyEvent
        {
            add
            {
                this.InternalEvent += value;
            }

            remove
            {
                this.InternalEvent -= value;
            }
        }

        public void MyMethod(string parameter) 
        {

        }
    }
}
