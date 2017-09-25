# 야매장기(Yame Janggi)

## Introduction
This is an implementation of deep-learning AI for Janggi(Korean Chess). All documents, comments and other description are written in Korean.

Janggi : https://en.wikipedia.org/wiki/Janggi

야매장기는 알파고에서 강하게 영감을 받아 딥러닝으로 구현한 장기 AI입니다. 딥러닝은 텐서플로우로 구현되었고, 나머지 게임과 MinMax, 강화 학습 등은 WPF 및 C#으로 제작 중입니다.

## Features

  알파고의 구현 원리를 참고하되, 학습 방법에서 다소 저의 편의대로 변형이 있을 수 있습니다. 누군가 같이 개발해주면 좋겠지만 아직 특별히 도움을 기대하지 않으므로 저 나름 재미나게 만들 계획입니다.

## Environment / Requirement

 - Visual Studio 2017
 - .Net Framework 4.7
 - Windows 10
 - Python 3.6 64bit

모든 개발은 Visual Studio 2017에서 이루어졌습니다. C#/WPF 코드는 .NET 4.6.2 환경에서 만들었습니다. TensorFlow를 이용한 딥러닝 알고리즘은 Python 3.6에서 작동됩니다.

## Goal

우선적인 목표는 장기도사를 이기는 건데 잘 될지 모르겠네요 ㅎㅎ 장기도사가 워낙 쎄서...

## 프로젝트 구성

 - genMoveSet : 별 거 아닌 보조 프로젝트. 2450가지의 움직임을 생성합니다. 여기서 생성된 결과를 소스 코드에 복사해서 사용했습니다.
 - Janggi : 메인 알고리즘이 구현된 프로젝트입니다.
 - RunnerConsole : 콘솔 환경에서 실행하는 것들이 포함되어있습니다. 간단한 테스트에서부터 지도학습, 강화학습 등이 포함됩니다.
 - RunnerWpf : GUI환경에서 동작합니다. 장기를 직접 두어볼 수 있고, 컴퓨터의 생각 트리를 확인할 수 있습니다.
 - TensorFlow : 텐서 플로우 프로젝트입니다. TCP/IP로 Janggi 라이브러리와 통신합니다.

현재는 개발단계라서 매끄러운 실행이 어려울 수 있습니다. RunnerWpf와 TensorFlow 두 프로젝트를 동시에 띄우면 개발된 결과를 확인할 수 있습니다. 초기 로딩에 상당한 시간이 걸립니다.