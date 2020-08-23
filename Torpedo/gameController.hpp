#ifndef GAMECONTROLLER_HPP_INCLUDED
#define GAMECONTROLLER_HPP_INCLUDED
#include <vector>

using namespace std;

class gameController
{
protected:
    vector <int> playerFieldStatus;
    vector <int> enemyFieldStatus;
    vector <vector<int>> enemyShipStatus;
    vector <vector<int>> playerShipStatus;
    vector <bool> placed;
    int inRow;
    int N;
public:
    gameController();
    virtual bool hit(int i,bool isPlayerTurn);
    virtual bool isGoodTarget(int i, bool isPlayerTurn);
    virtual vector<int> getEnemyValue();
    virtual vector<int> getPlayerValue();
    virtual void placingEnemyShips();
    virtual bool placingPlayerShips(int shipNumber,int first, int direction, int length,bool isShip);
    virtual bool whichSank(int i,bool isPlayer);
    virtual vector<vector<int>> getShipNumber();
    virtual void place(int i);
    virtual bool isPlaced(int i);
    virtual int AI();
};

#endif // GAMECONTROLLER_HPP_INCLUDED
