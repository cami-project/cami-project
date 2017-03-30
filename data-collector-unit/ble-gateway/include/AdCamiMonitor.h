//
//  AdCamiMonitor.h
//  AdCamiDaemon
//
//  Created by Jorge Miguel Miranda on 23/11/16.
//
//

#ifndef AdCamiDaemon_AdCamiMonitor_h
#define AdCamiDaemon_AdCamiMonitor_h

#include <sys/types.h>

#define READ_FD_SET (unsigned char) (1 << 0)
#define WRITE_FD_SET (unsigned char) (1 << 1)
#define EXCEPT_FD_SET (unsigned char) (1 << 2)

/**
 *
 */
typedef struct _DescriptorCallbackParameters {
    void* _caller;
    void* _parameter;
} DescriptorCallbackParameters;
/**
 *
 */
typedef int (*DescriptorCallback)(DescriptorCallbackParameters* params);


class AdCamiMonitor {
private:
    class FileDescriptorNode {
	public:
		int* fd;
		DescriptorCallback _callback;
		DescriptorCallbackParameters* _clbkParameters;
		unsigned char fd_types;
		FileDescriptorNode *_next;
		
		~FileDescriptorNode() {
			delete fd;
			delete _next;
		}
    };
    
	bool _running;
	FileDescriptorNode *_fdList;
	/* Global descriptors sets. */
	//int _timeout;
	fd_set _rfds;
	fd_set _wfds;
	fd_set _efds;
	/* Variables to hold temporary file descriptors. */
	fd_set* _t_rfds;
	fd_set* _t_wfds;
	fd_set* _t_efds;
	int* _t_max;
	struct timeval* _timeout;
    DescriptorCallback _t_callback;
    DescriptorCallbackParameters* _t_parameters;


public:
	AdCamiMonitor();
	~AdCamiMonitor();

	int AddDescriptor(const int& fd, unsigned char fdType,
                      DescriptorCallback clbk, DescriptorCallbackParameters* params);
	int AddDescriptor(fd_set* rfds, fd_set* wfds, fd_set* efds, int* maxfd,
                      DescriptorCallback clbk, DescriptorCallbackParameters* params);
	int RemoveDescriptor(const int& fd);
	/*void setTemporaryFDSET(fd_set readFds, fd_set writeFds, int max,
                           DescriptorCallback clbk, DescriptorCallbackParameters* params);*/
	int Start();
	int Start(struct timeval& timeout);
};

#endif /* AdCamiDaemon_AdCamiMonitor_h */
