/*
Copyright 2022 Lee Taek Hee (Tech University of Korea)

This program is free software: you can redistribute it and/or modify
it under the terms of the What The Hell License. Do it plz.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY.
*/

#include "stdafx.h"
#include <iostream>
#include "Dependencies\glew.h"
#include "Dependencies\freeglut.h"

#include "Renderer.h"

Renderer *g_Renderer = NULL;
bool g_bNeedReloadShaderPrograms = false;

void RenderScene(void)
{
	if (g_bNeedReloadShaderPrograms)
	{
		g_Renderer->ReloadAllShaderPrograms();
		g_bNeedReloadShaderPrograms = false;
	}

	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
	//g_Renderer->DrawFullScreenColor(0, 0, 0, 0.5);
	glClearColor(0.0f, 0.0f, 0.0f, 0.0f);

	// Renderer Test
	//g_Renderer->DrawSolidRect(0, 0, 0, 10, 1, 0, 1, 1);
	//g_Renderer->DrawTest();
	//g_Renderer->DrawParticle();
	//g_Renderer->DrawGridMesh();
	//g_Renderer->DrawFS();
	//g_Renderer->DrawTexture(0, 0, 1, 1, 0);
	//g_Renderer->DrawFBOs();
	g_Renderer->DrawBloomParticle();
	g_Renderer->DrawDebugTexture();


	glutSwapBuffers();
}

void Idle(void)
{
	RenderScene();
}

void MouseInput(int button, int state, int x, int y)
{
	RenderScene();
}

void KeyInput(unsigned char key, int x, int y)
{
	RenderScene();
}

void SpecialKeyInput(int key, int x, int y)
{
	switch (key) {
	case '1':
		g_bNeedReloadShaderPrograms = true;
		break;
	default:
		break;
	}
}

int main(int argc, char **argv)
{
	// window size
	int winX = 512;
	int winY = 512;

	// Initialize GL things
	glutInit(&argc, argv);
	glutInitDisplayMode(GLUT_DEPTH | GLUT_DOUBLE | GLUT_RGBA);
	glutInitWindowPosition(0, 0);
	glutInitWindowSize(winX, winY);
	glutCreateWindow("Game Software Engineering KPU");

	glewInit();
	if (glewIsSupported("GL_VERSION_3_0"))
	{
		std::cout << " GLEW Version is 3.0\n ";
	}
	else
	{
		std::cout << "GLEW 3.0 not supported\n ";
	}

	// Initialize Renderer
	g_Renderer = new Renderer(winX, winY);
	if (!g_Renderer->IsInitialized())
	{
		std::cout << "Renderer could not be initialized.. \n";
	}

	glutDisplayFunc(RenderScene);
	glutIdleFunc(Idle);
	glutKeyboardFunc(KeyInput);
	glutMouseFunc(MouseInput);
	glutSpecialFunc(SpecialKeyInput);

	glutMainLoop();

	delete g_Renderer;

    return 0;
}

