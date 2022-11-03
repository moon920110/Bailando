""" Define the functions to load data. """
import os
import shutil
import argparse
import numpy as np

from easydict import EasyDict
import yaml

from tqdm import tqdm


pose_keypoints_num = 25
face_keypoints_num = 70
hand_left_keypoints_num = 21
hand_right_keypoints_num = 21


class VSConfig():
    height = 540
    width = 960*2
    ckpt_epoch = 10
    set_epoch = False


def write2txt(dances, dance_names, config, expdir, epoch):
    epoch = int(epoch)
    assert len(dances) == len(dance_names),\
        "number of generated dance != number of dance_names"

    if config.set_epoch:
        ep_path = os.path.join(expdir, "txts", f'ep{epoch:06d}')
    else:
        ep_path = os.path.join(expdir, "txts")
        
    if not os.path.exists(ep_path):
        os.makedirs(ep_path)

    # print("Writing TxT...")
    for i in tqdm(range(len(dances)),desc='Generating Txts'):
        num_poses = dances[i].shape[0]
        dances[i] = dances[i].reshape(num_poses, pose_keypoints_num, 3)
        dance_path = os.path.join(ep_path, dance_names[i])
        if not os.path.exists(dance_path):
            os.makedirs(dance_path)

        for j in range(num_poses):
            frame_dict = {'version': 1.2}
            # 2-D key points
            pose_keypoints_2d = []
            frame_txt = np.zeros((17, 3))

            keypoints = dances[i][j]

            for k, keypoint in enumerate(keypoints):
                x = (keypoint[0] + 1) * 0.5 * config.width
                y = (keypoint[1] + 1) * 0.5 * config.height
                z = (keypoint[2] + 1) * 0.5 * config.height

                score = 0.8
                if k < pose_keypoints_num:
                    pose_keypoints_2d.extend([x, y, z])
            #         # pose_keypoints_2d.extend([x, y, score])
            pose_keypoints_2d = np.array(pose_keypoints_2d).reshape(25, 3)

            # hip
            frame_txt[0] = pose_keypoints_2d[8]
            # right Leg
            frame_txt[1] = pose_keypoints_2d[12]
            frame_txt[2] = pose_keypoints_2d[13]
            frame_txt[3] = pose_keypoints_2d[14]
            # left Leg
            frame_txt[4] = pose_keypoints_2d[9]
            frame_txt[5] = pose_keypoints_2d[10]
            frame_txt[6] = pose_keypoints_2d[11]

            # spine
            if pose_keypoints_2d[1][0] > pose_keypoints_2d[8][0]:
                frame_txt[7][0] = pose_keypoints_2d[1][0] - np.abs(pose_keypoints_2d[1][0]-pose_keypoints_2d[8][0])/2
            else:
                frame_txt[7][0] = pose_keypoints_2d[8][0] - np.abs(pose_keypoints_2d[1][0]-pose_keypoints_2d[8][0])/2

            if pose_keypoints_2d[1][1] > pose_keypoints_2d[8][1]:
                frame_txt[7][1] = pose_keypoints_2d[1][1] - np.abs(pose_keypoints_2d[1][1]-pose_keypoints_2d[8][1])/2
            else:
                frame_txt[7][1] = pose_keypoints_2d[8][1] - np.abs(pose_keypoints_2d[1][1]-pose_keypoints_2d[8][1])/2

            if pose_keypoints_2d[1][2] > pose_keypoints_2d[8][2]:
                frame_txt[7][2] = pose_keypoints_2d[1][2] - np.abs(pose_keypoints_2d[1][2]-pose_keypoints_2d[8][2])/2
            else:
                frame_txt[7][2] = pose_keypoints_2d[8][2] - np.abs(pose_keypoints_2d[1][2]-pose_keypoints_2d[8][2])/2
            
            # chest
            frame_txt[8] = pose_keypoints_2d[1]
            # neck
            frame_txt[9] = pose_keypoints_2d[0]
            # head
            frame_txt[10] = pose_keypoints_2d[16]
            
            # frame_txt[10][2] = np.array([frame_txt[9][2],frame_txt[16][2]]).mean()
            # right leg
            frame_txt[11] = pose_keypoints_2d[2]
            frame_txt[12] = pose_keypoints_2d[3]
            frame_txt[13] = pose_keypoints_2d[4]
            # left leg
            frame_txt[14] = pose_keypoints_2d[5]
            frame_txt[15] = pose_keypoints_2d[6]
            frame_txt[16] = pose_keypoints_2d[7]
            frame_txt = np.transpose(frame_txt).tolist()
            frame_txt = [frame_txt]

            with open(os.path.join(dance_path, f'{j}.txt'), 'w') as f:
                f.writelines("%s" % frame_txt)


def visualizeAndWritefromPKL(pkl_root, config=None):
    if config is None:
        config = VSConfig()
    dance_names = []
    np_dances = []
    np_dances_original = []

    epoch = config.ckpt_epoch
    if config.set_epoch:
        pkl_path = os.path.join(pkl_root, f'ep{epoch:06d}')
    else:
        pkl_path = pkl_root

    for pkl_name in os.listdir(pkl_path):
        if os.path.isdir(os.path.join(pkl_path, pkl_name)):
            continue
        result = np.load(os.path.join(pkl_path, pkl_name), allow_pickle=True).item()['pred_position']
        dance_names.append(pkl_name)

        np_dance = result

        root = np_dance[:, :3]
        np_dance[:, :3] = root
        np_dances_original.append(np_dance)

        if len(np_dance.shape) == 2:
            b, c = np_dance.shape
        else:
            np_dance = np_dance[:, :24]
            b, c, _ = np_dance.shape

        dimension = 3

        np_dance = np_dance.reshape([b, 24, 3])
        np_dance = np_dance[:b]
        np_dance -= np_dance[:1, :1, :]
        np_dance2 = np_dance[:, :, :dimension] / 1.5
        np_dance2[:, :, 0] /= 2.2
        np_dance_trans = np.zeros([b, 25, dimension]).copy()
        
        # head
        np_dance_trans[:, 0] = np_dance2[:, 12]
        
        #neck
        np_dance_trans[:, 1] = np_dance2[:, 9]
        
        # left up
        np_dance_trans[:, 2] = np_dance2[:, 16]
        np_dance_trans[:, 3] = np_dance2[:, 18]
        np_dance_trans[:, 4] = np_dance2[:, 20]

        # right up
        np_dance_trans[:, 5] = np_dance2[:, 17]
        np_dance_trans[:, 6] = np_dance2[:, 19]
        np_dance_trans[:, 7] = np_dance2[:, 21]

        
        np_dance_trans[:, 8] = np_dance2[:, 0]
        
        np_dance_trans[:, 9] = np_dance2[:, 1]
        np_dance_trans[:, 10] = np_dance2[:, 4]
        np_dance_trans[:, 11] = np_dance2[:, 7]

        np_dance_trans[:, 12] = np_dance2[:, 2]
        np_dance_trans[:, 13] = np_dance2[:, 5]
        np_dance_trans[:, 14] = np_dance2[:, 8]

        np_dance_trans[:, 15] = np_dance2[:, 15]
        np_dance_trans[:, 16] = np_dance2[:, 15]
        np_dance_trans[:, 17] = np_dance2[:, 15]
        np_dance_trans[:, 18] = np_dance2[:, 15]

        np_dance_trans[:, 19] = np_dance2[:, 11]
        np_dance_trans[:, 20] = np_dance2[:, 11]
        np_dance_trans[:, 21] = np_dance2[:, 8]

        np_dance_trans[:, 22] = np_dance2[:, 10]
        np_dance_trans[:, 23] = np_dance2[:, 10]
        np_dance_trans[:, 24] = np_dance2[:, 7]

        np_dances.append(np_dance_trans.reshape([b, 25*dimension]))
    
    write2txt(np_dances, dance_names, config, pkl_root, epoch)

    img_dir = os.path.join(pkl_root, "imgs", f"ep{123221:06d}")
    if os.path.exists(img_dir):
        shutil.rmtree(img_dir)


if __name__ == '__main__':
    """
    #   path: Set the custom path. If it is None, it'll be automatically set to experiment pkl directory 
    #   config: If you don't want to use config file, then set up the VSConfig
    #   set_epoch: If it is false, epoch directory will be ignore
    """
    parser = argparse.ArgumentParser()
    parser.add_argument('--path')
    parser.add_argument('--config')
    parser.add_argument('--set-epoch', action='store_true')
    args = parser.parse_args()

    with open(args.config) as f:
        config = yaml.load(f)
    config = EasyDict(config)
    config_test = config.testing
    config_test.set_epoch = args.set_epoch

    if args.path is not None:
        file_path = args.path
    else:
        file_path = f'./experiments/{config.expname}/eval/pkl/'

    visualizeAndWritefromPKL(file_path, config_test)
