#include "AdCamiMonitor.h"
#include <sys/select.h>
#include <sys/time.h>
#include <unistd.h>
#include <cerrno>
#include <cstring>
#include "AdCamiUtilities.h"

AdCamiMonitor::AdCamiMonitor() : _running(true), _fdList(nullptr),
		_t_rfds(new fd_set), _t_wfds(new fd_set), _t_efds(new fd_set),
		_t_max(new int(0)), _timeout(nullptr), _t_callback(nullptr), _t_parameters(nullptr) {
	FD_ZERO(&_rfds);
	FD_ZERO(&_wfds);
	FD_ZERO(&_efds);

	FD_ZERO(_t_rfds);
	FD_ZERO(_t_wfds);
	FD_ZERO(_t_efds);
}

AdCamiMonitor::~AdCamiMonitor() {
	/* Stop select. */
	this->_running = false;
	/* Delete list of file descriptors. */
	FileDescriptorNode *pfdl = this->_fdList;
	while (this->_fdList) {
		this->_fdList = pfdl->_next;
		delete pfdl;
		pfdl = this->_fdList;
	}
	PRINT_DEBUG("Stopped monitor.")
}

int AdCamiMonitor::AddDescriptor(const int& fd, unsigned char fdType,
							   DescriptorCallback clbk, DescriptorCallbackParameters* params) {
	FileDescriptorNode *pfdl;

	if (fd < 0 ||  clbk == nullptr) {
		return -1;
	}

	pfdl = new FileDescriptorNode();

	pfdl->fd = const_cast<int*>(&fd);
	pfdl->fd_types = fdType;
	pfdl->_callback = clbk;
	pfdl->_clbkParameters = params;
	pfdl->_next = this->_fdList;
	this->_fdList = pfdl;

	return 0;
}

int AdCamiMonitor::AddDescriptor(fd_set* rfds, fd_set* wfds, fd_set* efds, int* maxfd,
							   DescriptorCallback clbk, DescriptorCallbackParameters* params) {
	this->_t_rfds = rfds;
	this->_t_wfds = wfds;
	this->_t_efds = efds;
	this->_t_max = maxfd;
	this->_t_callback = clbk;
    this->_t_parameters = params;

	return 0;
}

int AdCamiMonitor::RemoveDescriptor(const int& fd) {
	FileDescriptorNode *pfdl = this->_fdList;
	FileDescriptorNode *previous = this->_fdList;

	if(fd < 0) {
		return -1;
	}

	while(pfdl) {
		if(fd == *pfdl->fd) {
			if(pfdl != previous) {
				previous->_next = pfdl->_next;
			}
			else {
				this->_fdList = pfdl->_next;
			}
			delete pfdl;
			return 0;
		}
		previous = pfdl;
		pfdl = pfdl->_next;
	}

	return 0;
}

int AdCamiMonitor::Start() {
	FileDescriptorNode *pfdl = this->_fdList;
	int maxfd = 0;
	//struct timeval timeout_tv;
	int res;

	/*if(this->_timeout > 0) {
		timeout_tv.tv_sec = (*this->_timeout / 1000);
		timeout_tv.tv_usec = ((*this->_timeout % 1000) / 1000);

		PRINT_DEBUG("Nao devia chegar aqui 2 !")
	}*/

	while(this->_running) {
		/* Put pointer on the begining of the list. */
		pfdl = this->_fdList;
        
		/* Reset the file set */
		FD_ZERO(&this->_rfds);
		FD_ZERO(&this->_wfds);
		FD_ZERO(&this->_efds);

		/* Update fds */
		maxfd = *this->_t_max;
		this->_rfds = *this->_t_rfds;
		this->_wfds = *this->_t_wfds;
		this->_efds = *this->_t_efds;

		/* Add the files descriptors to the file set and get the maximum */
		while(pfdl) {
			if (pfdl->fd != nullptr) {
				if (pfdl->fd_types & READ_FD_SET) {
					FD_SET(*pfdl->fd, &this->_rfds);
				}

				if (pfdl->fd_types & WRITE_FD_SET) {
					FD_SET(*pfdl->fd, &this->_wfds);
				}

				if (pfdl->fd_types & EXCEPT_FD_SET) {
					FD_SET(*pfdl->fd, &this->_efds);
				}

				if (*pfdl->fd > maxfd) {
					maxfd = *pfdl->fd;
				}
			}
			pfdl = pfdl->_next;
		}
		PRINT_DEBUG("Waiting on select...")
		res = select(maxfd + 1, &this->_rfds, &this->_wfds, &this->_efds, this->_timeout);
		PRINT_DEBUG("Interrupt received")
		/* The system call was interrupted by a signal  and a signal handler was executed.
		 * Restart the interrupted system call. */
		if((res < 0) && (EINTR == errno)) {
			PRINT_DEBUG("error")
			continue;
		}
		else {
			/* Read error */
			if (res < 0) {
				PRINT_DEBUG("Aborting! errno = " << strerror(errno))
				return -1;
			}
			/* Read timeout */
			else if(res == 0) {
				PRINT_DEBUG("Nao devia chegar aqui 3 !")
				return -1;
			}
			else if ((res > 0) && (EINTR != errno)) {
				/* Check which descriptor received interrupt and for each call
				 * its callback. */
				pfdl = this->_fdList;
				while (pfdl != NULL) {
					if (FD_ISSET(*pfdl->fd, &this->_rfds) || //read descriptors
						FD_ISSET(*pfdl->fd, &this->_efds) || //exception descriptors
						FD_ISSET(*pfdl->fd, &this->_wfds)) { //write descriptors
//						PRINT_DEBUG("r = " << FD_ISSET(*pfdl->fd, &this->_rfds) << "; w = " << FD_ISSET(*pfdl->fd, &this->_wfds) << "; e = " << FD_ISSET(*pfdl->fd, &this->_efds))
						pfdl->_callback(pfdl->_clbkParameters);
						PRINT_DEBUG("exited callback");
						//res -= 1;
					}
					pfdl = pfdl->_next;
				}
				/* Call temporary callback if is the case */
				if(res > 0 && this->_t_callback) {
					this->_t_callback(this->_t_parameters);
					//res -= 1;
				}
			}
		}//else
	}//while

	PRINT_DEBUG("exiting monitor")
	return 0;
}

int AdCamiMonitor::Start(struct timeval& timeout) {
	this->_timeout = &timeout;

	this->Start();

	return 0;
}
