namespace ZorkOne.Interface;

internal interface IRandomChooser
{
    T Choose<T>(List<T> items);
}