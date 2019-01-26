import argparse

parser = argparse.ArgumentParser()

parser.add_argument('--model-policy', type=str, default='resnet')
parser.add_argument('--model-value', type=str, default='resnet')
parser.add_argument('--gibo-path', type=str, default='d:/dataset/gibo/')

args = parser.parse_args()
