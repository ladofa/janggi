# 야매장기 TensorFlow 프로젝트
TensorFlow로 구현된 파이썬 프로젝트입니다.

## 개발 환경
### 소프트웨어
다른 환경에서 테스트는 안 해봤습니다.
 - Ubuntu 16
 - python 3.8.5
 - TensorFlow 2.5.0
### Multi GPU
기본적으로 Multi GPU를 지원합니다. GPU가 하나여도 안 돌아가진 않을 것 같아요.
## 구성
 - 지도학습
 - Self-play 강화학습
## 지도학습 실행방법
### TfRecord 생성
.gib 파일을 이용해서 TfRecord를 만듭니다. .gib파일은 고대로부터(??) 전해 내려오는 장기 기보를 기록하기 위한 파일 형식으로서 장기도사 홈페이지나 기타 커뮤니티 사이트에서 받아올 수 있습니다. 문제는 표준이 명확하지 않기 때문에 제가 만든 파서가 여타 다른 파일에서 제대로 동작한다는 보장이 없습니다. 저는 장기도사 홈페이지에 올라온 기보를 대충 긁어모았습니다. 어쨌든 .gib 파일을 이용해서 TfRecord를 만드는 방법은 다음과 같습니다.
```bash
python3 run_gen_tfrecord.py \
    --gibo-train-dir=${HOME}/gibo_train \
    --gibo-val-dir=${HOME}/gibo_val \
    --record-train-path=${HOME}/records/gibo_train.tfrecord \
    --record-val-path=${HOME}/records/gibo_val.tfrecord
```
### 학습 진행
학습은 run_train_self.py를 이용합니다.

[이하 작성중]
