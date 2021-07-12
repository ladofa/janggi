# 야매장기(Yame Janggi)

![장기 그림](https://github.com/ladofa/janggi/blob/master/intro.jpg)

## Introduction
This is an implementation of deep-learning AI for Janggi(Korean Chess). All documents, comments and other description are written in Korean.

야매장기는 알파 제로 기반의 장기 구현입니다. 모든 학습은 TensorFlow/Keras 에서 이루어지며, 윈도우 UI 부분은 WPF로 구현됩니다. 과거 알파고 C# 기반의 프로그램을 전면적으로 수정중에 있습니다. 개발 및 사용법과 관련된 자세한 설명은 각 폴더의 README.md를 참고하시기 바랍니다.


## 진행 상황
2021년 7월 5일 현재
### 완료됨
 - 기보 기반의 supervised learning
 - MCTS를 활용한 강화 학습

### 계획됨
 - 학습 진행 상황 기록(TensorBoard)
 - RESTful API 서비스 및 해당 서비스를 활용하는 WPF 응용
   - 현재 상태 입력 -> 다음 수 출력 -> UI와 대전
   - MCTS 트리 내용 출력 -> view

## 구성
 - python_tf : Python/TensorFlow - 학습, 서비스
 - Janggi : Visual Studio - 레거시

## 감사의 말
본 프로젝트는 "2021년 NIPA 인공지능 고성능컴퓨팅 지원사업"의 도움으로 진행되었습니다.

