#%%
import torch
from torch import nn

class Residual(nn.Module):
    def __init__(self, ch=256):
        super().__init__()
        self.main = nn.Sequential(
            nn.Conv2d(ch, ch, 3, padding=1),
            nn.BatchNorm2d(ch),
            nn.ReLU(),
            nn.Conv2d(ch, ch, 3, padding=1),
            nn.BatchNorm2d(ch)
        )
    
    def forward(self, input):
        z = self.main(input)
        z = z + input
        z = torch.relu(z)
        return z

class PolicyValueNet(nn.Module):
    def __init__(self, in_ch=16, mid_ch=128, block_num=15, policy_num=2451):
        super().__init__()
        self.main = nn.Sequential(
            nn.Conv2d(in_ch, mid_ch, 3, padding=1),
            nn.BatchNorm2d(mid_ch),
            nn.ReLU(),
            *[Residual(mid_ch) for _ in range(block_num)]
        )

        self.policy = nn.Sequential(
            nn.Conv2d(mid_ch, 54, 1), #54 came from 2451 / (9 * 10) * 2
            nn.LazyBatchNorm2d(),
            nn.ReLU(),
            nn.Flatten(),
            nn.LazyLinear(policy_num)
        )

        self.value = nn.Sequential(
            nn.Conv2d(mid_ch, 2, 1),
            nn.LazyBatchNorm2d(),
            nn.ReLU(),
            nn.Flatten(),
            nn.LazyLinear(256),
            nn.ReLU(),
            nn.LazyLinear(1),
            nn.Tanh(),
        )
    
    def forward(self, input):
        z = self.main(input)
        return self.policy(z), self.value(z)
    
model = PolicyValueNet()
model.cuda()

#%%
def load(filename):
    pass

def save(filename):
    pass

def eval(input):
    input = torch.tensor(input, dtype=torch.float32).reshape(-1, 16, 10, 9)
    model.eval()
    with torch.no_grad():
        policy, value = model(input.cuda())
    return policy.cpu().numpy(), value.cpu().numpy()

#%%
if __name__ == '__main__':
    import torchsummary
    torchsummary.summary(model, (16, 10, 9))
# %%
