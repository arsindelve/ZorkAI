namespace Model.Interface;

public interface IRandomChooser
{
    T Choose<T>(List<T> items);
}