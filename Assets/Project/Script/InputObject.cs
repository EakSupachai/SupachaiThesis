
public class InputObject
{
    public string input;
    public float grad0;
    public float grad1;
    public float grad2;

    public InputObject(string input, float grad0, float grad1, float grad2)
    {
        this.input = input;
        this.grad0 = grad0;
        this.grad1 = grad1;
        this.grad2 = grad2;
    }
}
