import gibo
from params import args

if __name__ == '__main__':
    gibo.gen_tfrecord(args.gibo_train_dir, args.record_train_path)
    gibo.gen_tfrecord(args.gibo_val_dir, args.record_val_path)