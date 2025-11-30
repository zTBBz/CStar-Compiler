#ifndef VECTOR_H
#define VECTOR_H

#include <stdbool.h>
#include <stdint.h>


// --- Forward Declarations ---
typedef struct Vector_Vector Vector_Vector;

// --- Struct Definitions ---
struct Vector_Vector
{
    int32_t x;    
};


// --- Prototypes ---
void Vector_Test();

#endif // VECTOR_H
