#ifndef GAMELOGIC_H
#define GAMELOGIC_H

#include <stdbool.h>
#include <stdint.h>

#include "Math.h"
#include <OpenGl.h>

// --- Forward Declarations ---
typedef struct GameLogic_Player GameLogic_Player;

// --- Struct Definitions ---
struct GameLogic_Player
{
    int32_t health;    
    int32_t x;    
    int32_t y;    
    Player GameLogic_Create()
    {
        /* var inferred */ int instance = /* ? */;        
        /* ? */.health = 100;        
        /* ? */.x = 0;        
        /* ? */.y = 0;        
        return /* ? */;        
    }
    
};


// --- Prototypes ---
void GameLogic_Update(Player p);

#endif // GAMELOGIC_H
