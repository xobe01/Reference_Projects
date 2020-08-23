import java.io.IOException;
import java.util.List;
import java.util.Random;

public class Passenger extends Thread
{
    private int ID;
    private int stayTime;
    private Computer comp;
    boolean hasBoarded = false;
    SpaceShip spaceShip;

    public Passenger(int ID, Computer comp)
    {
        this.comp = comp;
        this.ID = ID;
        stayTime = new Random().nextInt(40)+20;
    }

    public void run() {
        System.out.println("Utas "+ID+" áthaladt a zsilipkapun");
        try {
            comp.WriteToFile("Utas "+ID+" áthaladt a zsilipkapun");
        } catch (IOException e) {
            e.printStackTrace();
        }
        try
        {
            sleep(stayTime*1000);
            System.out.println("Utas " +ID+ " vissza kíván térni a Földre");
            try {
                comp.WriteToFile("Utas " +ID+ " vissza kíván térni a Földre");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        catch (InterruptedException e)
        {
            e.printStackTrace();
        }
        comp.ApplyForLeave(this);
        synchronized (comp.GetApplyingPassengers())
        {
            while(!hasBoarded)
            {
                try
                {
                    comp.GetApplyingPassengers().wait();
                }
                catch (InterruptedException e)
                {
                    e.printStackTrace();
                }
            }
        }
        try {
            spaceShip.join();
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public void JoinShip(SpaceShip s)
    {
        spaceShip = s;
        hasBoarded=true;
        System.out.println("Utas "+ID+" visszahaladt a zsilipkapun az űrhajó " + s.GetID()+"-ra");
        try {
            comp.WriteToFile("Utas "+ID+" visszahaladt a zsilipkapun az űrhajó " + s.GetID()+"-ra");
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
