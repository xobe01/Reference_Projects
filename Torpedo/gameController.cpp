#include "gameController.hpp"
#include <vector>
#include <time.h>
#include <stdlib.h>

using namespace std;

gameController::gameController()
{
    N=100;
    inRow=10;
    vector <vector<int>>ships(5);
    vector<int>fields(N,0);
    vector<bool>place(5,false);
    enemyFieldStatus=fields;
    playerFieldStatus=fields;
    enemyShipStatus=ships;
    playerShipStatus=ships;
    placed=place;
}
    //0 - empty, untouched field
    //1 - ship placed, untouched field
    //2 - empty, already field
    //3 - ship placed, already hit field
bool gameController::hit(int i,bool isPlayerTurn)
{
    if(isPlayerTurn)
    {
        if(enemyFieldStatus[i]==0)
        {
            enemyFieldStatus[i]=2;
            return false;
        }
        else if(enemyFieldStatus[i]==1)
        {
            enemyFieldStatus[i]=3;
            return true;
        }
        else
        {
            return false;
        }
    }
    else
    {
        if(playerFieldStatus[i]==0)
        {
            playerFieldStatus[i]=2;
            return false;
        }
        else if(playerFieldStatus[i]==1)
        {
            playerFieldStatus[i]=3;
            return true;
        }
        else
        {
            return false;
        }
    }
}
bool gameController::isGoodTarget(int i,bool isPlayerTurn)
{
    if(i>N || i<0)
    {
        return false;
    }
    else
    {
        if(isPlayerTurn)
        {
            if(enemyFieldStatus[i]==0)
            {
                return true;
            }
            else if(enemyFieldStatus[i]==1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if(playerFieldStatus[i]==0)
            {
                return true;
            }
            else if(playerFieldStatus[i]==1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
vector <int> gameController::getEnemyValue()
{
    return enemyFieldStatus;
}
vector <int> gameController::getPlayerValue()
{
    return playerFieldStatus;
}
bool gameController::placingPlayerShips(int shipNumber,int first, int direction, int length,bool isShip)
{
    //direction 0 - horizontal
    //direction 1 - vertical
    if(direction==0 && (first+length-1)%inRow>=length-1)
    {
        bool fieldIsEmpty=true;
        for(int i=0;i<length;i++)
        {
            if(playerFieldStatus[first+i]!=0)
            {
                fieldIsEmpty=false;
            }
        }
        if(fieldIsEmpty)
        {
            if(isShip)
            {
                playerShipStatus[shipNumber].push_back(length);
                playerShipStatus[shipNumber].push_back(direction);
                for(int i=0;i<length;i++)
                {
                    playerFieldStatus[first+i]=1;
                    playerShipStatus[shipNumber].push_back(first+i);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    else if(direction==1 && first<N-(length-1)*inRow)
    {
        bool fieldIsEmpty=true;
        for(int i=0;i<length;i++)
        {
            if(playerFieldStatus[first+i*inRow]!=0)
            {
                fieldIsEmpty=false;
            }
        }
        if(fieldIsEmpty)
        {
            if(isShip)
            {
                playerShipStatus[shipNumber].push_back(length);
            playerShipStatus[shipNumber].push_back(direction);
            for(int i=0;i<length;i++)
            {
                playerFieldStatus[first+i*inRow]=1;
                playerShipStatus[shipNumber].push_back(first+i*inRow);
            }
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    else
    {
        return false;
    }
}
void gameController::placingEnemyShips()
{
    srand(time(NULL));
    for(int j=0;j<enemyShipStatus.size();j++)
    {
        int length=j+2;
        if(j>1)
        {
            length--;
        }
        bool done=false;
        while(!done)
        {
            int first=rand()%N;
            int direction=rand()%2;
            //direction 0 - horizontal
            //direction 1 - vertical
            if(direction==0 && (first+length-1)%inRow>=length-1)
            {
                bool fieldIsEmpty=true;
                for(int i=0;i<length;i++)
                {
                    if(enemyFieldStatus[first+i]!=0)
                    {
                        fieldIsEmpty=false;
                    }
                }
                if(fieldIsEmpty)
                {
                    enemyShipStatus[j].push_back(length);
                    enemyShipStatus[j].push_back(direction);
                    for(int i=0;i<length;i++)
                    {
                        enemyFieldStatus[first+i]=1;
                        enemyShipStatus[j].push_back(first+i);
                    }
                    done=true;
                }
            }
            else if(direction==1 && first<N-(length-1)*inRow)
            {
                bool fieldIsEmpty=true;
                for(int i=0;i<length;i++)
                {
                    if(enemyFieldStatus[first+i*inRow]!=0)
                    {
                        fieldIsEmpty=false;
                    }
                }
                if(fieldIsEmpty)
                {
                    enemyShipStatus[j].push_back(length);
                    enemyShipStatus[j].push_back(direction);
                    for(int i=0;i<length;i++)
                    {
                        enemyFieldStatus[first+i*inRow]=1;
                        enemyShipStatus[j].push_back(first+i*inRow);
                    }
                    done=true;
                }
            }
        }
    }
}
bool gameController::whichSank(int i, bool isPlayer)
{
    if(isPlayer)
    {
        bool isSunk=true;
        for (int j=2;j<playerShipStatus[i].size();j++)
        {
            if(playerFieldStatus[playerShipStatus[i][j]]==1)
            {
                isSunk=false;
            }
        }
        return isSunk;
    }
    else
    {
        bool isSunk=true;
        for (int j=2;j<enemyShipStatus[i].size();j++)
        {
            if(enemyFieldStatus[enemyShipStatus[i][j]]==1)
            {
                isSunk=false;
            }
        }
        return isSunk;
    }
}
vector<vector<int>> gameController::getShipNumber()
{
    //1. element - length
    //2. element - direction
    //others - coordinates
    return enemyShipStatus;
}
void gameController::place(int i)
{
    placed[i]=true;
}
bool gameController::isPlaced(int i)
{
    return placed[i];
}
int gameController::AI()
{
    int iCounter=0;
    int jCounter=0;
    int shortestShip=playerShipStatus[0][0];
    for(int i=0;i<playerShipStatus.size();i++)
    {
        if(!whichSank(i,true) && playerShipStatus[i][0]<shortestShip)
        {
            shortestShip=playerShipStatus[i][0];
        }
    }
    int target;
    vector <int> targets;
    for(int i=0;i<playerShipStatus.size();i++)
    {
        bool isOne=false;
        bool isThree=false;
        for(int j=2;j<playerShipStatus[i].size();j++)
        {
            if(playerFieldStatus[playerShipStatus[i][j]]==3)
            {
                isThree=true;
            }
            else if(playerFieldStatus[playerShipStatus[i][j]]==1)
            {
                isOne=true;
            }
        }
        if(isThree && isOne)
        {
            for(int j=2;j<playerShipStatus[i].size();j++)
            {
                if(playerFieldStatus[playerShipStatus[i][j]]==3)
                {
                    targets.push_back(playerShipStatus[i][j]);
                }
            }
        }
    }
    bool done=false;
    bool checkArea;
    bool randomTarget=false;
    while(!done)
    {
        checkArea=false;
        if(targets.size()==0)
        {
            target=rand()%N;
            if(isGoodTarget(target,false))
            {
                checkArea=true;
            }
        }
        else if(targets.size()==1)
        {
            randomTarget=true;
        }
        else if(targets.size()>1 && !randomTarget)
        {
            bool found=false;
            while(iCounter<targets.size() && !checkArea && !found)
            {
                while(jCounter<targets.size() && !checkArea && !found)
                {
                    if(targets[iCounter]-targets[jCounter]==inRow && targets[iCounter]/inRow>1)
                    {
                            if(isGoodTarget(targets[jCounter]-inRow,false))
                        {
                            target=targets[jCounter]-inRow;
                            checkArea=true;
                            found=true;
                        }
                    }
                    else if(targets[iCounter]-targets[jCounter]==-inRow && targets[iCounter]/inRow<inRow-2)
                    {
                        if(isGoodTarget(targets[jCounter]+inRow,false))
                        {
                            target=targets[jCounter]+inRow;
                            checkArea=true;
                            found=true;
                        }
                    }
                    else if(targets[iCounter]-targets[jCounter]==1 && targets[iCounter]%inRow>1)
                    {
                            if(isGoodTarget(targets[jCounter]-1,false))
                        {
                            target=targets[jCounter]-1;
                            checkArea=true;
                            found=true;
                        }
                    }
                    else if(targets[iCounter]-targets[jCounter]==-1 && targets[iCounter]%inRow<inRow-2)
                    {
                        if(isGoodTarget(targets[jCounter]+1,false))
                        {
                            target=targets[jCounter]+1;
                            checkArea=true;
                            found=true;
                        }
                    }
                    jCounter++;
                }
                if(!found)
                {
                    jCounter=0;
                    iCounter++;
                }
            }
            if(!checkArea)
            {
                randomTarget=true;
            }
        }
        if(randomTarget)
        {
            target=targets[rand()%targets.size()];
            int direction =rand()%4;
            //0 - Left
            //1 - Right
            //2 - Up
            //3 - Down
            if(direction==0 && target%inRow!=0 && isGoodTarget(target-1,false))
            {
                target--;
                checkArea=true;
            }
            else if(direction==1 && target%inRow!=inRow-1 && isGoodTarget(target+1,false))
            {
                target++;
                checkArea=true;
            }
            else if(direction==2 && target/inRow!=0 && isGoodTarget(target-inRow,false))
            {
                target-=inRow;
                checkArea=true;
            }
            else if(direction==3 && target/inRow!=inRow-1 && isGoodTarget(target+inRow,false))
                {
                target+=inRow;
                checkArea=true;
            }
        }
        if(checkArea)
        {
            int counter=1;
            int i=1;
            bool isGoodField=true;
            while(isGoodField)
            {
                bool isTarget=false;
                for(int j=0;j<targets.size();j++)
                {
                    if(targets[j]==target+i)
                    {
                        isTarget=true;
                    }
                }
                if((isGoodTarget(target+i,false) || isTarget) && (target+i)%inRow!=0)
                {
                    counter++;
                    i++;
                }
                else
                {
                    isGoodField=false;
                }
            }
            i=1;
            isGoodField=true;
            while(isGoodField)
            {
                bool isTarget=false;
                for(int j=0;j<targets.size();j++)
                {
                    if(targets[j]==target-i)
                    {
                        isTarget=true;
                    }
                }
                if((isGoodTarget(target-i,false) || isTarget) && (target-i)%inRow!=inRow-1)
                {
                    counter++;
                    i++;
                }
                else
                {
                    isGoodField=false;
                }
            }
            if(counter>=shortestShip)
            {
                done=true;
            }
            else
            {
                counter=1;
                i=1;
                isGoodField=true;
                while(isGoodField)
                {
                    bool isTarget=false;
                    for(int j=0;j<targets.size();j++)
                    {
                        if(targets[j]==target+i*inRow)
                        {
                            isTarget=true;
                        }
                    }
                    if((isGoodTarget(target+i*inRow,false) || isTarget) && (target+i*inRow)/inRow<=inRow-1)
                    {
                        counter++;
                        i++;
                    }
                    else
                    {
                        isGoodField=false;
                    }
                }
                i=1;
                isGoodField=true;
                while(isGoodField)
                {
                    bool isTarget=false;
                    for(int j=0;j<targets.size();j++)
                    {
                        if(targets[j]==target-i*inRow)
                        {
                            isTarget=true;
                        }
                    }
                    if((isGoodTarget(target-i*inRow,false) || playerFieldStatus[target-i*inRow]==3) && (target-i*inRow)/inRow>=0)
                    {
                        counter++;
                        i++;
                    }
                    else
                    {
                        isGoodField=false;
                    }
                }
                if(counter>=shortestShip)
                {
                    done=true;
                }
            }
        }
    }
    return target;
}
