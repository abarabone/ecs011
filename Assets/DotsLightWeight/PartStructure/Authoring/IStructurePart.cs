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
�E

�G���A
�E�n�`�t�B�b�g                                     �{�^��
�E���b�V���t�B�b�g�@�e�Z���[�g                      �@�{�^��
�E�p�X�ό`�͕`�惁�b�V���p�@�f�u���̓V�F�[�_�ŕό`       
�@- �f�u���̓v���n�u�t�F�[�Y�Ŏ��O����


�E�p�X��ʁ@��ԁ^����                             �p�[�c�P��
�E�p�X���b�V���@���ϊ�                             �p�[�c�P��


�\�����@�G���A
�E�f�u���@����^�Ȃ�                              �p�[�c�P��
�E�f�u���̓v���n�u�L�[�ˑ�
�E�{�[���@�t���[���^�Œ�o�b�t�@                    �\�����P��
�E�p���b�g
�E�אڃp�[�c
*/