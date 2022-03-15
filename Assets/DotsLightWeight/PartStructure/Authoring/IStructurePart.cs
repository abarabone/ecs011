using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DotsLite.Structure.Authoring
{
    using DotsLite.Geometry;

    public interface IStructurePart
    {
        int partId { get; set; }
        IEnumerable<IMeshModel> QueryModel { get; }
    }
}
/*
�G���A
�E�q�\����

�E�n�`�t�B�b�g                                     �{�^��     o
�E���b�V���t�B�b�g�@�e�Z���[�g                      �@�{�^��
�E�p�X�ό`�͕`�惁�b�V���p�@�f�u���̓V�F�[�_�ŕό`       
�@- �f�u���̓v���n�u�t�F�[�Y�Ŏ��O����


�E�p�X��ʁ@��ԁ^����                             �p�[�c�P��
�E�p�X���b�V���@���ϊ�                             �p�[�c�P��   o



�\�����@�G���A
�E�p�[�c�P�ʂ̃G���e�B�e�B�^�{�[���P�ʂ̃G���e�B�e�B

�E�f�u���@����^�Ȃ�                              �p�[�c�P��
�E�f�u���̓v���n�u�L�[�ˑ�

�E�{�[���@�t���[���^�Œ�o�b�t�@                    �\�����P��

�E�p���b�g
�E�אڃp�[�c



�p���b�g�t�����f��
�E�V�[���ŋ��ʂ̃o�b�t�@
�E���f�����Ƃ� base index
�E���_�^�e�N�X�`���t�F�b�`���Ƃ� sub index
pallets[base index + sub index]

uns[] ... �A�g���X���̋�`�v�f�̌���
  offset
  range

6 bytes
up, left, forward
down, right, back

pos     x,y,z
size    x,y,z
rot     x,y,z,w
indices
 pallet sub index   ulf-, drb-
 uv index           ulf-, drb-
 

 
*/