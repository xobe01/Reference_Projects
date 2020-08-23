import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class SpaceShip extends Thread
{
    private boolean isEmpty;
    private int ID;
    private int weight;
    private int capacity;
    private List<Passenger> passengers = new ArrayList<>();
    private Computer comp;
    private boolean optimalize;
    private Dock usedDock;

    SpaceShip(int ID, Computer comp, boolean isEmpty)
    {
        usedDock = null;
        optimalize = true;
        this.isEmpty=isEmpty;
        this.ID=ID;
        this.comp = comp;
        weight = new Random().nextInt(900)+100;
        if(!isEmpty)
        {
            capacity = new Random().nextInt(90)+10;
            int currentID = 1;
            for(int i=0;i<capacity;i++)
            {
                Passenger p = new Passenger(this.ID*1000 + currentID, comp);
                passengers.add(p);
                currentID++;
            }
        }
        else
        {
            capacity = 50;
        }
    }

    public void run()
    {
        if(!isEmpty)
        {
            System.out.println("Űrhajó "+ID+"  elindult az ISS-re, " + capacity + " utassal, " +weight+ " súllyal");
            try {
                comp.WriteToFile("Űrhajó "+ID+"  elindult az ISS-re, " + capacity + " utassal, " +weight+ " súllyal");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        else
        {
            System.out.println("Űrhajó "+ID+"  elindult az ISS-re, " + capacity + " férőhellyel, " +weight+ " súllyal");
            try {
                comp.WriteToFile("Űrhajó "+ID+"  elindult az ISS-re, " + capacity + " férőhellyel, " +weight+ " súllyal");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        try {
            FindDock();
        } catch (IOException e) {
            e.printStackTrace();
        }
        if(!isEmpty)
        {
            GetOut();
        }
        else
        {
            GetIn();
        }
        if(!isEmpty && optimalize && comp.IsWorthWaiting())
        {
            System.out.println("Űrhajó "+ID+"  megvárja a visszatérő utasokat");
            try {
                comp.WriteToFile("Űrhajó "+ID+"  megvárja a visszatérő utasokat");
            } catch (IOException e) {
                e.printStackTrace();
            }
            GetIn();
        }
        usedDock.SetIsFree(true);
        System.out.println("Űrhajó "+ID+"  elindult az ISS-ről, " + passengers.size() + " utassal, " +weight+ " súllyal");
        try {
            comp.WriteToFile("Űrhajó "+ID+"  elindult az ISS-ről, " + passengers.size() + " utassal, " +weight+ " súllyal");
        } catch (IOException e) {
            e.printStackTrace();
        }
        comp.FreeSpace(passengers.size(),weight);
        try {
            comp.PrintNewValues();
        } catch (IOException e) {
            e.printStackTrace();
        }
        try
        {
            sleep(10000);
        }
        catch (InterruptedException e)
        {
            e.printStackTrace();
        }
    }

    private void FindDock() throws IOException {
        synchronized (comp.GetDocks())
        {
            boolean docked = false;
            boolean isWaiting=false;
            while(!docked)
            {
                for (Dock d:comp.GetDocks()) {
                    if (d.GetIsFree())
                    {
                        int realCapacity=0;
                        if (!isEmpty)
                        {
                            realCapacity = capacity;
                        }
                        else
                        {
                            realCapacity=0;
                        }
                        if (comp.IsFreeDock(realCapacity, weight))
                        {
                            System.out.println("Űrhajó " + ID + " dokkolt a(z) " + d.GetID() + ". dokkolóhoz");
                            comp.WriteToFile("Űrhajó " + ID + " dokkolt a(z) " + d.GetID() + ". dokkolóhoz");
                            comp.PrintNewValues();
                            d.SetIsFree(false);
                            usedDock = d;
                            docked = true;
                            if (isWaiting)
                            {
                                comp.MinusWaitingShips();
                            }
                            break;
                        }
                    }
                }
                if(!docked)
                {
                    try
                    {
                        System.out.println("Űrhajó "+ID+"  várakozik a dokkolásra");
                        comp.WriteToFile("Űrhajó "+ID+"  várakozik a dokkolásra");
                        comp.PrintNewValues();
                        if(!isWaiting)
                        {
                            comp.PlusWaitingShips();
                        }
                        isWaiting=true;
                        comp.GetDocks().wait();

                    }
                    catch (InterruptedException e)
                    {
                        e.printStackTrace();
                    }
                }
            }
        }
    }
    private void GetIn()
    {
        Passenger p=null;
        boolean isFull=false;
        while(!isFull)
        {
            synchronized (comp.GetApplyingPassengers())
            {
                if(comp.GetApplyingPassengers().size()>0 && passengers.size()<capacity)
                {
                    p = comp.GetApplyingPassengers().get(0);
                    passengers.add(p);
                    comp.RemoveOneApplicant();
                }
                else
                {
                    comp.GetApplyingPassengers().notifyAll();
                    isFull=true;
                }
            }
            if(!isFull)
            {
                try {
                    sleep(500);

                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                p.JoinShip(this);
            }
        }
    }
    private void GetOut()
    {
        for (Passenger p:passengers)
        {
            try
            {
                sleep(500);
                p.start();
            }
            catch (InterruptedException e)
            {
                e.printStackTrace();
            }
        }
        passengers.clear();
    }

    int GetID()
    {
        return ID;
    }
}

