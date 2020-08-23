#ifndef STATICTEXT_HPP_INCLUDED
#define STATICTEXT_HPP_INCLUDED
#include <string>
#include "widget.hpp"

using namespace std;

class staticText : public widget
{
private:
    string wText;
public:
    staticText(int x_, int y_, int side_,int frame_, string wText_);
    virtual void draw();
    virtual bool isSelected(int xMouse, int yMouse);
    virtual void drawAction(int enemyValue);
    virtual int getLength();
    virtual void beSelected();
    virtual void dontBeSelected();
    virtual int getX();
    virtual int getY();
};

#endif // STATICTEXT_HPP_INCLUDED
