import numpy as np
import argparse
import json, io, pickle, os
from PIL import Image
import cv2, copy, warnings
from scipy.optimize import curve_fit

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('--kpt-dir', type=str, default='D:\\CILAB\\과제\\CT\\청각\\Bailando2D\\dance')
    parser.add_argument('--hand-dir', type=str, default='D:\\CILAB\\과제\\CT\\청각\\Bailando2D\\hand')
    parser.add_argument('--save-dir', type=str, default='./test')
    return parser.parse_args()

def func(x, a, b, c):
    return a * x**2 + b * x + c

def linear(x, a, b):
    return a * x + b

def setColor(im, yy, xx, color):
    if len(im.shape) == 3:
        if (im[yy, xx] == 0).all():
            im[yy, xx, 0], im[yy, xx, 1], im[yy, xx, 2] = color[0], color[1], color[2]
        else:
            im[yy, xx, 0] = ((im[yy, xx, 0].astype(float) + color[0]) / 2).astype(np.uint8)
            im[yy, xx, 1] = ((im[yy, xx, 1].astype(float) + color[1]) / 2).astype(np.uint8)
            im[yy, xx, 2] = ((im[yy, xx, 2].astype(float) + color[2]) / 2).astype(np.uint8)
    else:
        im[yy, xx] = color[0]

def interpPoints(x, y):
    if abs(x[:-1] - x[1:]).max() < abs(y[:-1] - y[1:]).max():
        curve_y, curve_x = interpPoints(y, x)
        if curve_y is None:
            return None, None
    else:
        with warnings.catch_warnings():
            warnings.simplefilter("ignore")
            if len(x) < 3:
                popt, _ = curve_fit(linear, x, y)
            else:
                popt, _ = curve_fit(func, x, y)
                if abs(popt[0]) > 1:
                    return None, None
        if x[0] > x[-1]:
            x = list(reversed(x))
            y = list(reversed(y))
        #curve_x = np.linspace(x[0], x[-1], (x[-1]-x[0]))
        curve_x = np.linspace(x[0], x[-1], int(x[-1]-x[0]))
        if len(x) < 3:
            curve_y = linear(curve_x, *popt)
        else:
            curve_y = func(curve_x, *popt)
    return curve_x.astype(int), curve_y.astype(int)




class ConnectKptHand:
    def __init__(self, kpt_dir, hand_dir, save_dir, music, kpt_scaling=1.4, hand_scaling=0.5):
        # paths
        self.music = music
        self.kpt_dir = os.path.join(kpt_dir, self.music)
        self.hand_dir = os.path.join(hand_dir, self.music)
        self.save_dir = os.path.join(save_dir, self.music)
        if not os.path.exists(self.save_dir):
            os.makedirs(self.save_dir)
        # joint connection
        self.adj_hands = [[0, 1], [1, 2], [2, 3], [3, 4], [0, 5], [5, 6], [6, 7], [7, 8],
                    [5, 9], [9, 10], [10, 11], [11, 12], [9, 13], [13, 14], [14, 15],
                    [15, 16], [13, 17], [0, 17], [17, 18], [18, 19], [19, 20]]
        self.adj_kpts = [
            [0, 1], [1, 8],  # body
            [1, 2], [2, 3], [3, 4],  # right arm
            [1, 5], [5, 6], [6, 7],  # left arm
            [8, 9], [9, 10], [10, 11], [11, 24], [11, 22], [22, 23],  # right leg
            [8, 12], [12, 13], [13, 14], [14, 21], [14, 19], [19, 20]]  # left leg

        self.pose_color_list = [
            [153,  0, 51], [153,  0,  0],
            [153, 51,  0], [153,102,  0], [153,153,  0],
            [102,153,  0], [ 51,153,  0], [  0,153,  0],
            [  0,153, 51], [  0,153,102], [  0,153,153], [  0,153,153], [  0,153,153], [  0,153,153],
            [  0,102,153], [  0, 51,153], [  0,  0,153], [  0,  0,153], [  0,  0,153], [  0,  0,153]]

        self.hand_edge_list = [
            [0, 1, 2, 3, 4],
            [0, 5, 6, 7, 8],
            [0, 9, 10, 11, 12],
            [0, 13, 14, 15, 16],
            [0, 17, 18, 19, 20]
        ]
        self.hand_color_list = [
            [204, 0, 0], [163, 204, 0], [0, 204, 82], [0, 82, 204], [163, 0, 204]
        ]
        self.kpt_scaling = kpt_scaling
        self.hand_scaling = hand_scaling
        self.img_shape = (1920, 1080)

    def _extract_valid_keypoints(self, pts):
        p = pts.shape[0]
        thre = 0.01
        output = np.zeros((p, 2))

        valid = (pts[:, 2] > thre)
        output[valid, :] = pts[valid, :2]

        return output

    def read_data(self):
        kpt_files = os.listdir(self.kpt_dir)
        self.kpts_seq = np.zeros((len(kpt_files)//2, 25, 3))
        idx = 0
        for i, kpt_file in enumerate(kpt_files):
            if i % 2 == 0:
                continue
            with io.open(os.path.join(self.kpt_dir, kpt_file), 'rb') as f:
                jd = json.load(f)
                dance = np.array(jd['people'][0]['pose_keypoints_2d'])
                dance = np.reshape(dance, (25, 3))
            self.kpts_seq[idx] = dance
            idx += 1

        hand_files = os.listdir(self.hand_dir)
        self.hands_seq = np.zeros((len(hand_files), 42, 3))
        for j, hand_file in enumerate(hand_files):
            with io.open(os.path.join(self.hand_dir, hand_file), 'rb') as f2:
                jd2 = json.load(f2)
                hand = np.array(jd2['keypoints_3d'])
            if len(hand) == 21:
                self.hands_seq[j, :21] = hand
            else:
                self.hands_seq[j] = hand

    def _drawEdge(self, im, x, y, bw=1, color=(255, 255, 255), draw_end_points=False):
        if x is not None and x.size:
            h, w = im.shape[0], im.shape[1]
            # edge
            for i in range(-bw, bw):
                for j in range(-bw, bw):
                    yy = np.maximum(0, np.minimum(h - 1, y + i))
                    xx = np.maximum(0, np.minimum(w - 1, x + j))
                    setColor(im, yy, xx, color)

            # edge endpoints
            if draw_end_points:
                for i in range(-bw * 2, bw * 2):
                    for j in range(-bw * 2, bw * 2):
                        if (i ** 2) + (j ** 2) < (4 * bw ** 2):
                            yy = np.maximum(0, np.minimum(h - 1, np.array([y[0], y[-1]]) + i))
                            xx = np.maximum(0, np.minimum(w - 1, np.array([x[0], x[-1]]) + j))
                            setColor(im, yy, xx, color)

    def _match_hand(self, kpt_hands, hands):
        hands = hands[:, :2]
        diff = hands[0] - kpt_hands
        hands -= diff
        return hands

    def _connect_keypoints(self, pts, random_drop_prob=0):
        pose_pts, hand_pts = pts

        h, w = self.img_shape
        output_edges = np.zeros((h, w, 3), np.uint8)

        if random_drop_prob > 0:
            # add random noise to keypoints
            pose_pts[[0, 15, 16, 17, 18], :] += 5 * np.random.randn(5, 2)

        ### pose
        for i, edge in enumerate(self.adj_kpts):
            x, y = pose_pts[edge, 0], pose_pts[edge, 1]
            if (np.random.rand() > random_drop_prob) and (0 not in x):
                curve_x, curve_y = interpPoints(x, y)
                self._drawEdge(output_edges, curve_x, curve_y, bw=3, color=self.pose_color_list[i], draw_end_points=True)

        if np.sum(hand_pts) != 0:
            hand_pts[:21, :-1] = self._match_hand(pose_pts[4], hand_pts[:21])
            hand_pts[21:, :-1] = self._match_hand(pose_pts[7], hand_pts[21:])
            hand_pts_l = hand_pts[:21]
            hand_pts_r = hand_pts[21:]
            for hand_pts in [hand_pts_l, hand_pts_r]:  # for left and right hand
                if np.random.rand() > random_drop_prob:
                    for i, edge in enumerate(self.hand_edge_list):  # for each finger
                        for j in range(0, len(edge) - 1):  # for each part of the finger
                            sub_edge = edge[j:j + 2]
                            x, y = hand_pts[sub_edge, 0], hand_pts[sub_edge, 1]
                            if 0 not in x:
                                line_x, line_y = interpPoints(x, y)
                                self._drawEdge(output_edges, line_x, line_y, bw=1, color=self.hand_color_list[i],
                                         draw_end_points=True)

        return output_edges

    def draw_kpts(self, start_timestep=0):
        end_timestep = len(self.hands_seq) - start_timestep
        h, w = self.img_shape
        h_ind = 0

        for i, kpts in enumerate(self.kpts_seq):
            pose_img = np.zeros((h, w, 3), np.uint8)
            pose_pts = self._extract_valid_keypoints(kpts)

            if (i >= start_timestep) and (i < end_timestep):
                hands = np.trunc(copy.deepcopy(self.hands_seq[h_ind])) * self.hand_scaling

                pts = [pose_pts, hands]
                h_ind += 1
            else:
                pts = [pose_pts, 0]

            pose_img += self._connect_keypoints(pts, random_drop_prob=0)

            img = Image.fromarray(pose_img)
            img = img.transpose(Image.FLIP_TOP_BOTTOM)
            img = np.asarray(img)
            img = cv2.cvtColor(img, cv2.COLOR_BGR2BGRA)
            img = Image.fromarray(np.uint8(img))
            tozero = 9 - len(str(i))
            fn = '0' * tozero + str(i) + '.png'
            img.save(os.path.join(self.save_dir, fn))


    def make_video(self):
        cmd = f"ffmpeg -r 15 -start_number 0 -i {self.save_dir}/%09d.png -vb 20M -vcodec mpeg4 -y test3.mp4 -loglevel quiet"
        # cmd = f"ffmpeg -r 60 -i {image_dir}/{dance}/%05d.png -vb 20M -vcodec qtrle -y {video_dir}/{name}.mov -loglevel quiet"
        os.system(cmd)

if __name__ == '__main__':
    args = parse_args()
    clips = os.listdir(args.kpt_dir)
    for clip in clips:
        print(clip)
        connector = ConnectKptHand(args.kpt_dir, args.hand_dir, args.save_dir, clip)
        #connector.read_data()
        #connector.draw_kpts(start_timestep=0)
        connector.make_video()



