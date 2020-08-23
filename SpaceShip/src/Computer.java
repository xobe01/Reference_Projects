import java.io.*;
import java.util.ArrayList;
import java.util.List;
import java.util.Random;

public class Computer extends Thread
{
    private int newApplicants;
    private int waitingShips;
    private int currentID;
    private int currentCapacity;
    private int currentCarryingCapacity;
    final private List<Dock> docks = new ArrayList<>();
    private List<Passenger> applyingPassengers = new ArrayList<>();
    private File file = new File("log.txt");

    Computer()
    {
        file.delete();
        newApplicants = 0;
        waitingShips=0;
        currentID=1;
        currentCapacity = 250;
        currentCarryingCapacity = 2500;
        for (int i=1;i<=5;i++)
        {
            docks.add(new Dock(i));
        }
    }

    boolean IsWorthWaiting()
    {
        if(currentCapacity<100 && applyingPassengers.size()>10)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    List<Passenger> GetApplyingPassengers()
    {
        synchronized (applyingPassengers)
        {
            return applyingPassengers;
        }
    }

    void PlusWaitingShips()
    {
        waitingShips++;
        if(waitingShips>=10)
        {
            System.out.println("Túl sok hajó várakozik, egy ideig csak üreseket küldenek");
            try {
                WriteToFile("Túl sok hajó várakozik, egy ideig csak üreseket küldenek");
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }

    void MinusWaitingShips()
    {
        waitingShips--;
    }

    void ApplyForLeave(Passenger pas)
    {
        newApplicants++;
        applyingPassengers.add(pas);
        if(newApplicants>=50)
        {
            newApplicants-=50;
            SpaceShip s = new SpaceShip(currentID,this,true);
            s.start();
            currentID++;
        }
    }

    void RemoveOneApplicant()
    {
        synchronized (applyingPassengers)
        {
            applyingPassengers.remove(0);
        }
    }

    boolean IsFreeDock(int capacity, int weight)
    {
        if(currentCapacity-capacity>=0 && currentCarryingCapacity-weight>=0)
        {
            currentCapacity-=capacity;
            currentCarryingCapacity-=weight;
            return true;
        }
        else
        {
            return false;
        }
    }

    void FreeSpace(int capacity, int weight)
    {
        synchronized (docks)
        {
            currentCapacity+=capacity;
            currentCarryingCapacity+=weight;
            docks.notifyAll();
        }
    }

    void PrintNewValues() throws IOException {
        System.out.println("Az ISS súlya: "+(2500-currentCarryingCapacity)+" és kapacitása: "+currentCapacity +" a dokkolásra várakozó hajók száma: "+waitingShips);
        WriteToFile("Az ISS súlya: "+(2500-currentCarryingCapacity)+" és kapacitása: "+currentCapacity +" a dokkolásra várakozó hajók száma: "+waitingShips);
    }

    List<Dock> GetDocks()
    {
        return docks;
    }

    public void run()
    {
        while(true)
        {
            if(waitingShips<=10)
            {
                SpaceShip s = new SpaceShip(currentID, this, false);
                s.start();
                currentID++;
            }
            try
            {
                int sleepTime=new Random().nextInt(5)+5;
                sleep(sleepTime*1000);
            }
            catch (InterruptedException e)
            {
                e.printStackTrace();
            }
        }
    }

    void WriteToFile(String s) throws IOException
    {
        FileWriter fileWriter = new FileWriter(file,true);
        BufferedWriter bw = new BufferedWriter(fileWriter);
        PrintWriter printWriter = new PrintWriter(bw);
        bw.write(s);
        bw.newLine();
        bw.close();
        printWriter.close();
        fileWriter.close();
    }

    public static void main (String[] args)
    {
        Computer comp = new Computer();
        comp.start();
    }
}
