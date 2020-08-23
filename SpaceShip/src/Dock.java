public class Dock
{
    private int ID;
    private boolean isFree;

    public  Dock(int ID)
    {
        this.ID = ID;
        isFree = true;
    }

    public boolean GetIsFree()
    {
        return isFree;
    }

    public void SetIsFree(boolean isFree)
    {
        this.isFree=isFree;
    }

    public int GetID()
    {
        return ID;
    }
}
