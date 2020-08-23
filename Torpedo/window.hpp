#ifndef WINDOW_HPP_INCLUDED
#define WINDOW_HPP_INCLUDED
#include <vector>
#include "widget.hpp"

using namespace std;

class window
{
protected:
    vector <widget*> playerFields;
    vector <widget*> enemyFields;
    vector <widget*> playerShips;
    vector <widget*> enemyShips;
    vector <widget*> staticShips;
    vector <widget*> staticTexts;
};

#endif // WINDOW_HPP_INCLUDED
