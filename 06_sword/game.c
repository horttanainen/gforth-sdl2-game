// This code is based on Gigi's Empty SDL2 window and handling keyboard and 
// mouse events tutorials:
// http://gigi.nullneuron.net/gigilabs/showing-an-empty-window-in-sdl2/
// http://gigi.nullneuron.net/gigilabs/handling-keyboard-and-mouse-events-in-sdl2/

#include <stdbool.h>
#include <string.h>
#include <SDL2/SDL.h>
#include "game-time.h"
#include "mathc.h"

typedef struct vec3 Vector;

typedef struct {
  Vector pos;
  Vector angularVelocity;
  Vector angularMomemtum;
  float inertia;
  float mass;
  Vector linearVelocity;
  Vector linearMomemtum;
  float distance;
} SwordPoint;

typedef struct {
  Vector pos;
  Vector angularVelocity;
  Vector angularMomemtum;
  float inertia;
  float mass;
  Vector linearVelocity;
  Vector linearMomemtum;
  Vector force;
} Player;

typedef struct {
  double dt;
  Timespec t;
  double acc;
  Timespec fpsUpdate;
} Time;

typedef struct {
  Player player;
  SwordPoint swordPoint;
} Scene;

SDL_Texture * loadTexture(SDL_Renderer * renderer) {
  SDL_Surface * image = SDL_LoadBMP("man.bmp");
  SDL_Texture * texture = SDL_CreateTextureFromSurface(renderer, image);
  SDL_FreeSurface(image);
  return texture;
}

Scene initScene() {
  Player player = {
    .pos = {
      .x = 288,
      .y = 208
    },
    .angularVelocity = {
      .x = 0,
      .y = 0
    },
    .angularMomemtum = {
      .x = 0,
      .y = 0
    },
    .linearVelocity = {
      .x = 0,
      .y = 0
    },
    .linearMomemtum = {
      .x = 0,
      .y = 0
    },
    .mass = 100,
    .inertia = 100,
    .force = {
      .x = 0,
      .y = 0,
      .z = 0
    },
  };
  float swordLength = 128;
  SwordPoint swordPoint = {
    .pos = {
      .x = 288 - swordLength,
      .y = 208 + 32
    },
    .angularVelocity = {
      .x = 0,
      .y = 0
    },
    .angularMomemtum = {
      .x = 0,
      .y = 0
    },
    .linearVelocity = {
      .x = 0,
      .y = 0
    },
    .linearMomemtum = {
      .x = 0,
      .y = 0
    },
    .mass = 10,
    .inertia = 10,
    .distance = swordLength
  };
  Scene scene = {
    .player = player,
    .swordPoint = swordPoint
  };
  return scene;
}

Time initTime() {
  Time time = {
    .dt = 0.01,
    .t = currentTime(),
    .acc = 0.01,
    .fpsUpdate = currentTime()
  };
  return time;
}

void handleKeyboard(Player * player) {
  float strength = 10000;
  const Uint8 * keyboardState = SDL_GetKeyboardState(NULL);
  if (keyboardState[SDL_SCANCODE_A] && keyboardState[SDL_SCANCODE_D]) {
    player->force.x = 0;
  } else if (keyboardState[SDL_SCANCODE_A]) {
    player->force.x = -strength;
  } else if (keyboardState[SDL_SCANCODE_D]) {
    player->force.x = strength;
  } else {
    player->force.x = 0;
  }
  if (keyboardState[SDL_SCANCODE_W] && keyboardState[SDL_SCANCODE_S]) {
    player->force.y = 0;
  } else if (keyboardState[SDL_SCANCODE_W]) {
    player->force.y = -strength;
  } else if (keyboardState[SDL_SCANCODE_S]) {
    player->force.y = strength;
  } else {
    player->force.y = 0;
  }
}

void handleEvents(bool * quit) {
  SDL_Event event;
  SDL_PollEvent(&event);
  switch (event.type)
  {
    case SDL_QUIT:
      *quit = true;
      break;
  }
}

int cleanUpAndExit(SDL_Texture * texture, SDL_Renderer * renderer, SDL_Window * window) {
  SDL_DestroyTexture(texture);
  SDL_DestroyRenderer(renderer);
  SDL_DestroyWindow(window); 
  SDL_Quit();
  return 0;
}

void integrate(SDL_Renderer * renderer, Scene * scene, double dt) {
  Player * player = &scene->player;
  SwordPoint * swordPoint = &scene->swordPoint;

  float massC = player->mass + swordPoint->mass;

  player->linearMomemtum = svec3_add(player->linearMomemtum, svec3_multiply_f(player->force, dt));
  player->linearVelocity = svec3_divide_f(player->linearMomemtum, massC);

  swordPoint->linearMomemtum = svec3_add(swordPoint->linearMomemtum, svec3_multiply_f(player->force, dt));
  swordPoint->linearVelocity = svec3_divide_f(swordPoint->linearMomemtum, massC);

  Vector posC = svec3_add(
      svec3_multiply_f(player->pos, player->mass / massC),
      svec3_multiply_f(swordPoint->pos, swordPoint->mass / massC)
      );

  SDL_SetRenderDrawColor(renderer, 255, 0, 0, SDL_ALPHA_OPAQUE);
  SDL_RenderDrawLine(renderer, 0 , 0, posC.x, posC.y);

  Vector fromCToPlayer = svec3_subtract(player->pos, posC);
  Vector fromCToPlayerPos = svec3_add(posC, fromCToPlayer);

  SDL_SetRenderDrawColor(renderer, 0, 255, 0, SDL_ALPHA_OPAQUE);
  SDL_RenderDrawLine(renderer, posC.x , posC.y, fromCToPlayerPos.x, fromCToPlayerPos.y);

  SDL_SetRenderDrawColor(renderer, 0, 0, 255, SDL_ALPHA_OPAQUE);
  SDL_RenderDrawLine(renderer, 0 , 0, player->pos.x, player->pos.y);

  Vector torque = svec3_cross(fromCToPlayer, svec3_divide_f(player->force, 10000));

  player->angularMomemtum = svec3_add(player->angularMomemtum, svec3_multiply_f(torque, dt));

  player->angularVelocity = svec3_divide_f(player->angularMomemtum, player->inertia);

  Vector playerPointVelocity = svec3_add(player->linearVelocity, svec3_cross(player->angularVelocity, fromCToPlayer));

  player->pos = svec3_add(player->pos,  svec3_multiply_f(playerPointVelocity, dt));

  swordPoint->angularMomemtum = svec3_add(swordPoint->angularMomemtum, svec3_multiply_f(torque, dt));
  swordPoint->angularVelocity = svec3_divide_f(swordPoint->angularMomemtum, swordPoint->inertia);

  Vector fromSwordPointToC = svec3_subtract(swordPoint->pos, posC);

  Vector swordPointVelocity = svec3_add(swordPoint->linearVelocity, svec3_cross(swordPoint->angularVelocity, fromSwordPointToC));

  swordPoint->pos = svec3_add(swordPoint->pos, svec3_multiply_f(swordPointVelocity, dt));

  Vector fromPlayerToSwordPoint = svec3_subtract(swordPoint->pos, player->pos);

  float currentDistance = svec3_length(fromPlayerToSwordPoint);

  if (currentDistance > swordPoint->distance + 10) {
    swordPoint->pos = svec3_add(player->pos, svec3_multiply_f(fromPlayerToSwordPoint, swordPoint->distance / currentDistance));
  }
}

void render(Scene * scene, SDL_Texture * texture, SDL_Renderer * renderer) {
  Player * player = &scene->player;
  SwordPoint * swordPoint = &scene->swordPoint;

  SDL_Rect dstrect = { player->pos.x, player->pos.y, 64, 64 };
  SDL_RenderCopy(renderer, texture, NULL, &dstrect);

  SDL_SetRenderDrawColor(renderer, 0, 0, 0, SDL_ALPHA_OPAQUE);
  SDL_RenderDrawLine(renderer, swordPoint->pos.x , swordPoint->pos.y, player->pos.x + 32, player->pos.y + 32);
  SDL_SetRenderDrawColor(renderer, 255, 255, 255, SDL_ALPHA_OPAQUE);

  SDL_RenderPresent(renderer);
  SDL_RenderClear(renderer);
}

void fps(Time * time, SDL_Window * window) {
  static int frames = 0;
  double timeSinceLastUpdate = diffTime(currentTime(), time->fpsUpdate);
  if ( timeSinceLastUpdate > 1 ) {
    char caption[80];
    sprintf(caption, "FPS: %d", frames);
    SDL_SetWindowTitle(window, caption);
    time->fpsUpdate = currentTime();
    frames = 0;
  }
  ++frames;
}

int main(int argc, char ** argv)
{
  SDL_Init(SDL_INIT_VIDEO);

  bool quit = false;
  SDL_Window * window = SDL_CreateWindow(
      "Game",
      SDL_WINDOWPOS_UNDEFINED,
      SDL_WINDOWPOS_UNDEFINED,
      640,
      480,
      0);
  SDL_Renderer * renderer = SDL_CreateRenderer(window, -1, 0);
  SDL_Texture * texture = loadTexture(renderer);

  Scene curScene = initScene();
  Scene prevScene = curScene;

  Time time = initTime();

  SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);

  while (!quit)
  {
    Timespec newTime = currentTime();
    double frameTime = diffTime(newTime, time.t);

    if ( frameTime > 0.25 ) {
      frameTime = 0.25;
    }
    time.t = newTime;

    time.acc += frameTime;

    while ( time.acc >= time.dt )
    {
      prevScene = curScene;
      integrate(renderer, &curScene, time.dt);
      time.t = timespecAdd(secondsToTimespec(time.dt), time.t);
      time.acc -= time.dt;
    }

    const double alpha = time.acc / time.dt;

    Scene interpolatedScene = {
      .player = {
        .pos = svec3_add(
            svec3_multiply_f(curScene.player.pos, alpha), 
            svec3_multiply_f(prevScene.player.pos, 1.0 - alpha)
            )
      },
      .swordPoint = {
        .pos = svec3_add(
            svec3_multiply_f(curScene.swordPoint.pos, alpha), 
            svec3_multiply_f(prevScene.swordPoint.pos, 1.0 - alpha)
            )
      }
    };
    render(&interpolatedScene, texture, renderer);
    handleEvents(&quit);
    handleKeyboard(&curScene.player);
    fps(&time, window);
  }
  return cleanUpAndExit(texture, renderer, window);
}
