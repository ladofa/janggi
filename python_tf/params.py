import argparse

parser = argparse.ArgumentParser()

parser.add_argument('--gibo-train-dir', type=str, default='/home/ubuntu/datasets/gibo3/train')
parser.add_argument('--gibo-val-dir', type=str, default='/home/ubuntu/datasets/gibo3/val')
parser.add_argument('--record-train-path', type=str, default='/home/ubuntu/datasets/gibo3/gibo_train.tfrecord')
parser.add_argument('--record-val-path', type=str, default='/home/ubuntu/datasets/gibo3/gibo_val.tfrecord')
parser.add_argument('--model-policy', type=str, default='resnet')
parser.add_argument('--model-value', type=str, default='resnet')
parser.add_argument('--batch-size', type=int, default=1024)
parser.add_argument('--filters', type=int, default=256)
parser.add_argument('--n-blocks', type=int, default=21)

parser.add_argument('--self-play-min-batch', type=int, default=512)
parser.add_argument('--self-play-episodes', type=int, default=32)
parser.add_argument('--mcts-max-travel-count', type=int, default=1)
parser.add_argument('--mcts-max-turn', type=int, default=150)


parser.add_argument('--travel-count', type=int, default=32)


args = parser.parse_args()
