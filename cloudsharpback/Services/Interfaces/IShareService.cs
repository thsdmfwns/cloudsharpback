﻿using cloudsharpback.Models;
using cloudsharpback.Models.DTO;
using cloudsharpback.Models.DTO.Member;
using cloudsharpback.Models.DTO.Share;

namespace cloudsharpback.Services.Interfaces
{
    public interface IShareService
    {
        Task<bool> CheckPassword(string token);
        Task<HttpResponseDto?> CloseShareAsync(MemberDto member, string token);
        Task<HttpResponseDto?> DeleteShareAsync(string target, MemberDto member);
        Task<HttpResponseDto?> DeleteSharesInDirectory(MemberDto memberDto, string targetDirectoryPath);
        /// <returns>404 : share doesnt exist , 403 : bad password, 410 : expired share</returns>
        Task<(HttpResponseDto? err, FileDownloadTicketValue? ticketValue)> GetDownloadTicketValue(ShareDowonloadRequestDto req);
        /// <returns>410 : expired share </returns>
        Task<(HttpResponseDto? err, ShareResponseDto? result)> GetShareAsync(string token);
        Task<List<ShareResponseDto>> GetSharesAsync(MemberDto member);
        /// <returns>404 : no file for share</returns>
        Task<HttpResponseDto?> Share(MemberDto member, ShareRequestDto req);
        Task<HttpResponseDto?> UpdateShareAsync(ShareUpdateDto dto, string token, MemberDto member);
        /// <returns>404 : NotFound Share</returns>
        Task<(HttpResponseDto? err, bool? result)> ValidatePassword(string password, string token);

        Task<bool> CheckExistShareByTargetPath(string target, MemberDto member);

        Task<(HttpResponseDto? err, List<ShareResponseDto> shares)> FindSharesInDirectory(MemberDto memberDto,
            string targetDir);
    }
}