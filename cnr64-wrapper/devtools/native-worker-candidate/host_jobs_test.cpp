#include "pv7_host_jobs.hpp"
#include <fstream>
#include <iostream>
#include <iterator>
#include <string>
#include <thread>
using namespace std::chrono_literals;
using namespace pv7::host;
void Check(bool ok,const char* s){if(!ok)throw std::runtime_error(s);std::cout<<"PASS "<<s<<std::endl;}
int Wait(HostJobs& jobs,std::uint32_t id){
    auto deadline=std::chrono::steady_clock::now()+15s;
    int r; while((r=jobs.Poll(id))==0 && std::chrono::steady_clock::now()<deadline)std::this_thread::sleep_for(1ms);
    return r;
}
int main(int argc,char** argv) {
    HostJobs jobs;
    std::string str="123456789";
    auto crc=jobs.Submit(2,reinterpret_cast<const std::uint8_t*>(str.data()),str.size(),0);
    Check(crc!=0,"checksum submitted"); Check(Wait(jobs,crc)==5,"checksum length");
    std::uint8_t buf[4]{};
    Check(jobs.Copy(crc,buf,3)==-3,"short buffer does not consume result");
    Check(jobs.Copy(crc,buf,4)==4 && buf[0]==0x26 && buf[1]==0x39 && buf[2]==0xf4 && buf[3]==0xcb,"standard CRC32 check vector");
    Check(jobs.Poll(crc)==-1,"ticket consumed");
    std::string json="{\"a\":\"x\\\"[{}]\",\"b\":[false,true,null,42,-7,1.5],\"n\":18446744073709551615}";
    auto j=jobs.Submit(1,reinterpret_cast<const std::uint8_t*>(json.data()),json.size(),0);
    Check(j!=0,"JSON job submitted");
    int state=Wait(jobs,j);Check(state>5,"JSON tree generated");
    std::vector<std::uint8_t> data(state-1);
    Check(jobs.Copy(j,data.data(),data.size())==(int)data.size() && data[0]=='P' && data[3]==1 && data[4]==5,"wire header and root object");
    auto bad=jobs.Submit(1,reinterpret_cast<const std::uint8_t*>("{]"),2,0);
    Check(bad && Wait(jobs,bad)==-2,"invalid JSON error");
    Check(jobs.Release(bad)&&jobs.Poll(bad)==-1,"failed ticket released");
    Check(jobs.Submit(99,nullptr,0,0)==0,"unknown opcode rejected");
    Check(jobs.Submit(1,nullptr,1024u*1024u+1,0)==0,"oversized request rejected");
    for(int i=0;i<8;++i)Check(jobs.Submit(2,nullptr,0,0)!=0,"bounded ticket submission");
    Check(jobs.Submit(2,nullptr,0,0)==0,"ninth ticket rejected");
    if(argc>2) {
        HostJobs catalog;
        std::ifstream input(argv[1],std::ios::binary);
        std::vector<std::uint8_t> bytes((std::istreambuf_iterator<char>(input)),{});
        auto id=catalog.Submit(1,bytes.data(),bytes.size(),0);
        int size=Wait(catalog,id);
        Check(size>0,"live catalog native JSON parse");
        data.resize(size-1);
        Check(catalog.Copy(id,data.data(),data.size())==size-1,"catalog result copied");
        std::ofstream out(argv[2],std::ios::binary);out.write(reinterpret_cast<const char*>(data.data()),data.size());
        Check(out.good(),"catalog wire file written");
    }
}
