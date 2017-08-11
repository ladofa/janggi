# 야매장기(Quack Janggi)

## Introduction
This is an implementation of deep-learning AI for Janggi(Korean Chess).

Janggi : https://en.wikipedia.org/wiki/Janggi

야매장기는 알파고에서 강하게 영감을 받아 딥러닝으로 구현한 장기 AI입니다. 딥러닝은 텐서플로우로 구현되었고, 나머지 게임과 MinMax, 강화 학습 등은 WPF 및 C#으로 제작되었습니다.. 가 아니고 지금 공부하면서 만들고 있습니다.^^

## Features

우선 밝힐 것은 모든 구현은 자료를 참고하여 제 나름의 방식으로 구현한 것이므로 기본적인 알고리즘의 원리를 공부하는 데는 도움이 되지 않습니다.

일단 MCTS를 쓰지 않습니다. 효율적인 rollout을 구현하는데 많은 시간이 걸리기 때문입니다. 대신 알파고의 형세 판단 네트워크를 100% 신뢰하는 방법을 사용했습니다. MinMax트리에서 마지막 수를 누가 두었느냐에 따라 형세가 왔다갔다 하기 때문에 상대가 마지막으로 뒀던 형세 판단값과 자신이 둔 형세 판단값을 따로 구분하여 업데이트합니다.

## Goal

우선적인 목표는 장기도사를 이기는 건데 잘 될지 모르겠네요 ㅎㅎ 장기도사가 워낙 쎄서...